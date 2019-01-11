using System;
using System.Collections;
using Ether.Core.Constants;
using Ether.Tests.Infrastructure;
using Ether.Tests.Reporters;
using NUnit.Framework;

namespace Ether.Tests.Data
{
    public static class ETADataProvider
    {
        public static IEnumerable WorkItemActiveTimeTestsNoETA() => new TestCaseData[]
        {
            SimplePathWithResolved(nameof(AggregatedWorkitemsETAReporterTests.ShouldCorrectlyIdentifyActiveTimeWithoutEstimates)),
            SimplePathWithCodeReview(nameof(AggregatedWorkitemsETAReporterTests.ShouldCorrectlyIdentifyActiveTimeWithoutEstimates)),
            OnHoldPeriods(nameof(AggregatedWorkitemsETAReporterTests.ShouldCorrectlyIdentifyActiveTimeWithoutEstimates)),
            BlockedPeriods(nameof(AggregatedWorkitemsETAReporterTests.ShouldCorrectlyIdentifyActiveTimeWithoutEstimates)),
            BlockedByOnHoldTagPeriods(nameof(AggregatedWorkitemsETAReporterTests.ShouldCorrectlyIdentifyActiveTimeWithoutEstimates)),
            MixedUseOfBlockedAndOnHoldTags(nameof(AggregatedWorkitemsETAReporterTests.ShouldCorrectlyIdentifyActiveTimeWithoutEstimates)),
            UnBlockedByOnHoldTagUponActivationPeriods(nameof(AggregatedWorkitemsETAReporterTests.ShouldCorrectlyIdentifyActiveTimeWithoutEstimates)),
            UnBlockedUponActivationPeriods(nameof(AggregatedWorkitemsETAReporterTests.ShouldCorrectlyIdentifyActiveTimeWithoutEstimates)),
            WithoutWeekends(nameof(AggregatedWorkitemsETAReporterTests.ShouldCorrectlyIdentifyActiveTimeWithoutEstimates)),
            WithoutCountingMinutes(nameof(AggregatedWorkitemsETAReporterTests.ShouldCorrectlyIdentifyActiveTimeWithoutEstimates)),
            IfResolvedOnTheSameDay(nameof(AggregatedWorkitemsETAReporterTests.ShouldCorrectlyIdentifyActiveTimeWithoutEstimates))
        };

        public static IEnumerable WorkItemActiveTimeTestsWithETA() => new TestCaseData[]
        {
            SimplePathWithResolved(nameof(AggregatedWorkitemsETAReporterTests.ShouldCorrectlyIdentifyActiveTimeWithEstimates)),
            SimplePathWithCodeReview(nameof(AggregatedWorkitemsETAReporterTests.ShouldCorrectlyIdentifyActiveTimeWithEstimates)),
            OnHoldPeriods(nameof(AggregatedWorkitemsETAReporterTests.ShouldCorrectlyIdentifyActiveTimeWithEstimates)),
            BlockedPeriods(nameof(AggregatedWorkitemsETAReporterTests.ShouldCorrectlyIdentifyActiveTimeWithEstimates)),
            BlockedByOnHoldTagPeriods(nameof(AggregatedWorkitemsETAReporterTests.ShouldCorrectlyIdentifyActiveTimeWithEstimates)),
            MixedUseOfBlockedAndOnHoldTags(nameof(AggregatedWorkitemsETAReporterTests.ShouldCorrectlyIdentifyActiveTimeWithEstimates)),
            UnBlockedUponActivationPeriods(nameof(AggregatedWorkitemsETAReporterTests.ShouldCorrectlyIdentifyActiveTimeWithEstimates)),
            UnBlockedByOnHoldTagUponActivationPeriods(nameof(AggregatedWorkitemsETAReporterTests.ShouldCorrectlyIdentifyActiveTimeWithEstimates)),
            WithoutWeekends(nameof(AggregatedWorkitemsETAReporterTests.ShouldCorrectlyIdentifyActiveTimeWithEstimates)),
            WithoutCountingMinutes(nameof(AggregatedWorkitemsETAReporterTests.ShouldCorrectlyIdentifyActiveTimeWithEstimates)),
            IfResolvedOnTheSameDay(nameof(AggregatedWorkitemsETAReporterTests.ShouldCorrectlyIdentifyActiveTimeWithEstimates))
        };

        private static TestCaseData SimplePathWithResolved(string testPrefix)
        {
            var activatedOn = new DateTime(2018, 5, 28);
            var resolvedOn = new DateTime(2018, 5, 30);
            var updates = UpdateBuilder.Create()
                .Activated().On(activatedOn)
                .Then().Resolved(TestData.DummyMember).On(resolvedOn)
                .Build();

            var expectedDuration = resolvedOn.Subtract(activatedOn).Days;
            return new TestCaseData(updates, expectedDuration)
                .SetName($"{testPrefix}{nameof(SimplePathWithResolved)}");
        }

        private static TestCaseData SimplePathWithCodeReview(string testPrefix)
        {
            var activatedOn = new DateTime(2018, 5, 28);
            var codeReviewDate = new DateTime(2018, 5, 30);
            var resolvedOn = new DateTime(2018, 5, 31);
            var updates = UpdateBuilder.Create()
                .Activated().On(activatedOn)
                .Then().With(VSTSFieldNames.Tags, "code Review; tag; tag2", "").On(codeReviewDate)
                .Then().Resolved(TestData.DummyMember).On(resolvedOn)
                .Build();

            var expectedDuration = codeReviewDate.Subtract(activatedOn).Days;
            return new TestCaseData(updates, expectedDuration)
                .SetName($"{testPrefix}{nameof(SimplePathWithCodeReview)}");
        }

        private static TestCaseData OnHoldPeriods(string testPrefix)
        {
            var activatedOn = new DateTime(2018, 5, 28);
            var resolvedOn = new DateTime(2018, 6, 1);
            var updates = UpdateBuilder.Create()
                .Activated().On(activatedOn)
                .Then().New().On(activatedOn.AddDays(1))
                .Then().Activated().On(activatedOn.AddDays(2))
                .Then().New().On(activatedOn.AddDays(3))
                .Then().Resolved(TestData.DummyMember).On(resolvedOn)
                .Build();

            return new TestCaseData(updates, 2)
                .SetName($"{testPrefix}{nameof(OnHoldPeriods)}");
        }

        private static TestCaseData BlockedPeriods(string testPrefix)
        {
            var activatedOn = new DateTime(2018, 5, 28);
            var resolvedOn = new DateTime(2018, 6, 1);
            var updates = UpdateBuilder.Create()
                .Activated().On(activatedOn)
                .Then().With(VSTSFieldNames.Tags, "blocked; tag1; tag2", null).On(activatedOn.AddDays(1))
                .Then().With(VSTSFieldNames.Tags, "tag1; tag2", "blocked; tag1; tag2").On(activatedOn.AddDays(2))
                .Then().With(VSTSFieldNames.Tags, "Blocked; tag1; tag2", null).On(activatedOn.AddDays(3))
                .Then().Resolved(TestData.DummyMember).On(resolvedOn)
                .Build();

            return new TestCaseData(updates, 2)
                .SetName($"{testPrefix}{nameof(BlockedPeriods)}");
        }

        private static TestCaseData BlockedByOnHoldTagPeriods(string testPrefix)
        {
            var activatedOn = new DateTime(2018, 5, 28);
            var resolvedOn = new DateTime(2018, 6, 1);
            var updates = UpdateBuilder.Create()
                .Activated().On(activatedOn)
                .Then().With(VSTSFieldNames.Tags, "onhold; tag1; tag2", null).On(activatedOn.AddDays(1))
                .Then().With(VSTSFieldNames.Tags, "tag1; tag2", "OnHold; tag1; tag2").On(activatedOn.AddDays(2))
                .Then().With(VSTSFieldNames.Tags, "OnHold; tag1; tag2", null).On(activatedOn.AddDays(3))
                .Then().Resolved(TestData.DummyMember).On(resolvedOn)
                .Build();

            return new TestCaseData(updates, 2)
                .SetName($"{testPrefix}{nameof(BlockedByOnHoldTagPeriods)}");
        }

        private static TestCaseData UnBlockedUponActivationPeriods(string testPrefix)
        {
            var activatedOn = new DateTime(2018, 5, 28);
            var resolvedOn = new DateTime(2018, 6, 1);
            var updates = UpdateBuilder.Create()
                .Activated().On(activatedOn)
                .Then().With(VSTSFieldNames.Tags, "blocked; tag1; tag2", null).On(activatedOn.AddDays(1))
                .Then().New().On(activatedOn.AddDays(1))
                .Then().Activated().With(VSTSFieldNames.Tags, "tag1; tag2", "blocked; tag1; tag2").On(activatedOn.AddDays(2))
                .Then().With(VSTSFieldNames.Tags, "Blocked; tag1; tag2", "").On(activatedOn.AddDays(3))
                .Then().Resolved(TestData.DummyMember).On(resolvedOn)
                .Build();

            return new TestCaseData(updates, 2)
                .SetName($"{testPrefix}{nameof(UnBlockedUponActivationPeriods)}");
        }

        private static TestCaseData UnBlockedByOnHoldTagUponActivationPeriods(string testPrefix)
        {
            var activatedOn = new DateTime(2018, 5, 28);
            var resolvedOn = new DateTime(2018, 6, 1);
            var updates = UpdateBuilder.Create()
                .Activated().On(activatedOn)
                .Then().With(VSTSFieldNames.Tags, "onhold; tag1; tag2", null).On(activatedOn.AddDays(1))
                .Then().New().On(activatedOn.AddDays(1))
                .Then().Activated().With(VSTSFieldNames.Tags, "tag1; tag2", "onhold; tag1; tag2").On(activatedOn.AddDays(2))
                .Then().With(VSTSFieldNames.Tags, "On Hold; tag1; tag2", "").On(activatedOn.AddDays(3))
                .Then().Resolved(TestData.DummyMember).On(resolvedOn)
                .Build();

            return new TestCaseData(updates, 2)
                .SetName($"{testPrefix}{nameof(UnBlockedByOnHoldTagUponActivationPeriods)}");
        }

        private static TestCaseData MixedUseOfBlockedAndOnHoldTags(string testPrefix)
        {
            var activatedOn = new DateTime(2018, 5, 28);
            var resolvedOn = new DateTime(2018, 6, 2);
            var updates = UpdateBuilder.Create()
                .Activated().On(activatedOn)
                .Then().With(VSTSFieldNames.Tags, "onhold; tag1; tag2", null).On(activatedOn.AddDays(1))
                .Then().Activated().With(VSTSFieldNames.Tags, "blocked; tag1; tag2", "onhold; tag1; tag2").On(activatedOn.AddDays(2))
                .Then().With(VSTSFieldNames.Tags, "tag1; tag2", "blocked; tag1; tag2").On(activatedOn.AddDays(3))
                .Then().Resolved(TestData.DummyMember).On(resolvedOn)
                .Build();

            return new TestCaseData(updates, 2)
                .SetName($"{testPrefix}{nameof(MixedUseOfBlockedAndOnHoldTags)}");
        }

        private static TestCaseData WithoutWeekends(string testPrefix)
        {
            var activatedOn = new DateTime(2018, 5, 25);
            var resolvedOn = new DateTime(2018, 6, 6);
            var updates = UpdateBuilder.Create()
                .Activated().On(activatedOn)
                .Then().Resolved(TestData.DummyMember).On(resolvedOn)
                .Build();

            return new TestCaseData(updates, 8)
                .SetName($"{testPrefix}{nameof(WithoutWeekends)}");
        }

        private static TestCaseData WithoutCountingMinutes(string testPrefix)
        {
            var activatedOn = new DateTime(2018, 5, 28, 23, 34, 45);
            var resolvedOn = new DateTime(2018, 6, 6, 01, 23, 45);
            var updates = UpdateBuilder.Create()
                .Activated().On(activatedOn)
                .Then().Resolved(TestData.DummyMember).On(resolvedOn)
                .Build();

            var expectedDuration = resolvedOn.Subtract(activatedOn).Days;
            return new TestCaseData(updates, 7)
                .SetName($"{testPrefix}{nameof(WithoutCountingMinutes)}");
        }

        private static TestCaseData IfResolvedOnTheSameDay(string testPrefix)
        {
            var activatedOn = new DateTime(2018, 5, 28, 12, 34, 45);
            var resolvedOn = new DateTime(2018, 5, 28, 15, 34, 45);
            var updates = UpdateBuilder.Create()
                .Activated().On(activatedOn)
                .Then().Resolved(TestData.DummyMember).On(resolvedOn)
                .Build();

            var expectedDuration = resolvedOn.Subtract(activatedOn).Days;
            return new TestCaseData(updates, 1)
                .SetName($"{testPrefix}{nameof(IfResolvedOnTheSameDay)}");
        }
    }
}