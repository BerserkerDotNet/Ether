using System;
using System.Collections.Generic;
using System.Linq;
using Ether.Contracts.Types;
using Ether.ViewModels;
using Ether.Vsts;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;

namespace Ether.Tests.Classifiers
{
    [TestFixture]
    public class BaseWorkItemsClassifierTest
    {
        private BaseWorkItemsClassifier _classifier;

        [SetUp]
        public void SetUp()
        {
            _classifier = new DummyClassifier();
        }

        [Test]
        public void ShouldThrowExceptionIfRequestIsNull()
        {
            _classifier.Invoking(c => c.Classify(null))
                .Should()
                .Throw<ArgumentNullException>();
        }

        [Test]
        public void ShouldReturnNoneIfWorkItemIsNull()
        {
            var result = _classifier.Classify(new WorkItemResolutionRequest());

            result.Should().NotBeNull();
            result.Should().OnlyContain(r => r.IsNone);
        }

        [Test]
        public void ShouldReturnNoneIfTeamIsNull()
        {
            var workItem = Builder<WorkItemViewModel>.CreateNew().Build();
            var result = _classifier.Classify(new WorkItemResolutionRequest { WorkItem = workItem });

            result.Should().NotBeNull();
            result.Should().OnlyContain(r => r.IsNone);
        }

        [Test]
        public void ShouldReturnResolutionForSupportedType()
        {
            var workItem = Builder<WorkItemViewModel>.CreateNew()
                .With(w => w.Fields, new Dictionary<string, string>())
                .Build();
            workItem.Fields.Add(Constants.WorkItemTypeField, DummyClassifier.SupportedType);
            workItem.Updates = UpdateBuilder.Create().Resolved()
                .Build();

            var result = _classifier.Classify(new WorkItemResolutionRequest { WorkItem = workItem });

            result.Should().NotBeNull();
            result.Should().OnlyContain(r => !r.IsNone);
            result.Should().OnlyContain(r => r.Resolution == DummyClassifier.ExpectedResolution);
        }

        [Test]
        public void ShouldReturnNoneIfEmptyHistoryItems()
        {
            var workItem = Builder<WorkItemViewModel>.CreateNew()
                .With(w => w.Fields, new Dictionary<string, string>())
                .Build();
            workItem.Fields.Add(Constants.WorkItemTypeField, DummyClassifier.SupportedType);
            workItem.Updates = Enumerable.Empty<WorkItemUpdateViewModel>();

            var result = _classifier.Classify(new WorkItemResolutionRequest { WorkItem = workItem });

            result.Should().NotBeNull();
            result.Should().OnlyContain(r => r.IsNone);
        }

        [Test]
        public void ShouldReturnNoneIfWorkItemUpdatesAreNull()
        {
            var workItem = Builder<WorkItemViewModel>.CreateNew()
                .With(w => w.Fields, new Dictionary<string, string>())
                .Build();
            workItem.Fields.Add(Constants.WorkItemTypeField, DummyClassifier.SupportedType);
            workItem.Updates = null;

            var result = _classifier.Classify(new WorkItemResolutionRequest { WorkItem = workItem });

            result.Should().NotBeNull();
            result.Should().OnlyContain(r => r.IsNone);
        }

        [Test]
        public void ShouldReturnErrorResolutionIfException()
        {
            var workItem = Builder<WorkItemViewModel>.CreateNew()
                .With(w => w.Fields, new Dictionary<string, string>())
                .Build();
            workItem.Fields.Add(Constants.WorkItemTypeField, ExceptionWorkItemClassifier.SupportedType);
            workItem.Updates = UpdateBuilder.Create().Resolved()
                .Build();

            var classifier = new ExceptionWorkItemClassifier();
            var result = classifier.Classify(new WorkItemResolutionRequest { WorkItem = workItem });

            result.Should().NotBeNull();
            result.Should().OnlyContain(r => r.IsError);
            result.Should().OnlyContain(r => r.Reason == ExceptionWorkItemClassifier.ExpectedReason);
        }
    }
}
