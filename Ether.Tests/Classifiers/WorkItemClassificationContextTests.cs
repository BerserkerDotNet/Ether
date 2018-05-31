using System;
using System.Collections.Generic;
using System.Linq;
using Ether.Core.Constants;
using Ether.Core.Interfaces;
using Ether.Core.Models;
using Ether.Core.Models.VSTS;
using Ether.Core.Reporters.Classifiers;
using Ether.Core.Types;
using Ether.Tests.Infrastructure;
using FluentAssertions;
using NUnit.Framework;

namespace Ether.Tests.Classifiers
{
    [TestFixture]
    public class WorkItemClassificationContext_Should : BaseTest
    {
        private const string DummyResolution1 = "DummyResolution 1";
        private const string DummyResolution2 = "DummyResolution 2";

        private WorkItemClassificationContext _context;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            var classifiers = new IWorkItemsClassifier[]
            {
                new DummyClassifier(DummyResolution1),
                new DummyClassifier(DummyResolution2),
                new DummyClassifier(type: WorkItemTypes.Bug),
                new ExceptionWorkItemClassifier()
            };
            _context = new WorkItemClassificationContext(classifiers);
        }

        [Test]
        [TestData(RelatedWorkItemsPerMember = 2)]
        public void CallAllClassifier()
        {
            var result = Classify(DummyClassifier.SupportedType);

            result.Should().HaveCount(2);
            result.Should().NotContain(r => r.IsNone);
            result.Should().OnlyContain(r => r.Resolution == DummyResolution1 || r.Resolution == DummyResolution2);
        }

        [Test]
        [TestData(RelatedWorkItemsPerMember = 2)]
        public void PassCorrectClassificationRequest()
        {
            WorkItemResolutionRequest request = null;
            var c = new DummyClassifier(DummyResolution1, requestProcessor: r => request = r);
            var ctx = new WorkItemClassificationContext(new[] { c });

            var workitem = GetWorkitem(DummyClassifier.SupportedType);
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow.AddDays(-2);
            ctx.Classify(workitem, new ClassificationScope(Data.TeamMembers, startDate, endDate));

            request.Should().NotBeNull();
            request.Team.Should().BeEquivalentTo(Data.TeamMembers);
            request.WorkItem.Should().Be(workitem);
            request.StartDate.Should().Be(startDate);
            request.EndDate.Should().Be(endDate);
        }

        [Test]
        [TestData(RelatedWorkItemsPerMember = 2)]
        public void FilterOutEmptyClassificationResults()
        {
            var result = Classify();

            result.Should().HaveCount(1);
            result.Should().NotContain(r => r.IsNone);
            result.Should().OnlyContain(r => r.Resolution == DummyClassifier.ExpectedResolution);
        }

        [Test]
        [TestData(RelatedWorkItemsPerMember = 2)]
        public void ReturnResolutionsAsList()
        {
            var result = Classify();

            result.Should().BeOfType<List<WorkItemResolution>>();
        }

        [Test]
        [TestData(RelatedWorkItemsPerMember = 2)]
        public void NotFilterErrorClassificationResults()
        {
            var result = Classify(type: ExceptionWorkItemClassifier.SupportedType);

            result.Should().HaveCount(1);
            result.Should().OnlyContain(r => r.IsError);
        }

        [Test]
        [TestData(RelatedWorkItemsPerMember = 2)]
        public void FilterResultsByStartAndEndDate()
        {
            var workitem = GetWorkitem(DummyClassifier.SupportedType, resolvedOn: DateTime.UtcNow);
            var startDate = DateTime.UtcNow.AddDays(-3);
            var endDate = DateTime.UtcNow.AddDays(-2);
            var result = _context.Classify(workitem, new ClassificationScope(Data.TeamMembers, startDate, endDate));

            result.Should().BeEmpty();
        }

        private IEnumerable<WorkItemResolution> Classify(string type = WorkItemTypes.Bug)
        {
            var workitem = GetWorkitem(type, resolvedOn: DateTime.UtcNow.AddDays(-2));
            var startDate = DateTime.UtcNow.AddDays(-3);
            var endDate = DateTime.UtcNow;
            return _context.Classify(workitem, new ClassificationScope(Data.TeamMembers, startDate, endDate));
        }

        private VSTSWorkItem GetWorkitem(string type = WorkItemTypes.Bug, DateTime? resolvedOn = null)
        {
            var user = Data.TeamMembers.First();
            var workitem = Data.GetWorkItemFor(user, type);
            workitem.Updates = Data.GetBugFullCycle(resolvedBy: user, closedBy: user, resolvedOn: resolvedOn);

            return workitem;
        }
    }
}
