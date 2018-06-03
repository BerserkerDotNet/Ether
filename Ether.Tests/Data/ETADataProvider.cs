using System;
using System.Collections;
using Ether.Core.Constants;
using Ether.Tests.Infrastructure;
using NUnit.Framework;

namespace Ether.Tests.Data
{
    public static class ETADataProvider
    {
        public static IEnumerable WorkItemActiveTimeTests() => new TestCaseData[]
        {
            SimplePathWithResolved(),
            SimplePathWithCodeReview(),
            OnHoldPeriods(),
            BlockedPeriods(),
            UnBlockedUponActivationPeriods(),
            WithoutWeekends(),
            WithoutCountingMinutes()
        };

        private static TestCaseData SimplePathWithResolved()
        {
            var activatedOn = new DateTime(2018, 5, 28);
            var resolvedOn = new DateTime(2018, 5, 30);
            var updates = UpdateBuilder.Create()
                .Activated().On(activatedOn)
                .Then().Resolved(TestData.DummyMember).On(resolvedOn)
                .Build();

            var expectedDuration = resolvedOn.Subtract(activatedOn).Days;
            return new TestCaseData(updates, expectedDuration)
                .SetName($"ShouldCorrectlyIdentifyActiveTimeFor{nameof(SimplePathWithResolved)}");
        }

        private static TestCaseData SimplePathWithCodeReview()
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
                .SetName($"ShouldCorrectlyIdentifyActiveTimeFor{nameof(SimplePathWithCodeReview)}");
        }

        private static TestCaseData OnHoldPeriods()
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
                .SetName($"ShouldCorrectlyIdentifyActiveTimeWith{nameof(OnHoldPeriods)}");
        }

        private static TestCaseData BlockedPeriods()
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
                .SetName($"ShouldCorrectlyIdentifyActiveTimeWith{nameof(BlockedPeriods)}");
        }

        private static TestCaseData UnBlockedUponActivationPeriods()
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
                .SetName($"ShouldCorrectlyIdentifyActiveTimeWith{nameof(UnBlockedUponActivationPeriods)}");
        }


        private static TestCaseData WithoutWeekends()
        {
            var activatedOn = new DateTime(2018, 5, 25);
            var resolvedOn = new DateTime(2018, 6, 6);
            var updates = UpdateBuilder.Create()
                .Activated().On(activatedOn)
                .Then().Resolved(TestData.DummyMember).On(resolvedOn)
                .Build();

            return new TestCaseData(updates, 8)
                .SetName($"ShouldCorrectlyIdentifyActiveTime{nameof(WithoutWeekends)}");
        }

        private static TestCaseData WithoutCountingMinutes()
        {
            var activatedOn = new DateTime(2018, 5, 28, 23, 34, 45);
            var resolvedOn = new DateTime(2018, 6, 6, 01, 23, 45);
            var updates = UpdateBuilder.Create()
                .Activated().On(activatedOn)
                .Then().Resolved(TestData.DummyMember).On(resolvedOn)
                .Build();

            var expectedDuration = resolvedOn.Subtract(activatedOn).Days;
            return new TestCaseData(updates, 7)
                .SetName($"ShouldCorrectlyIdentifyActiveTime{nameof(WithoutCountingMinutes)}");
        }
    }
}