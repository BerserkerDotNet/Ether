using System;
using System.Collections.Generic;
using System.Linq;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Types;
using Ether.Core.Types;
using Ether.ViewModels;
using Ether.Vsts;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;

namespace Ether.Tests.Classifiers
{
    [TestFixture]
    public class WorkItemClassificationContextTests
    {
        private const string DummyResolution1 = "DummyResolution 1";
        private const string DummyResolution2 = "DummyResolution 2";

        private IEnumerable<TeamMemberViewModel> _teamMembers;
        private RandomGenerator _randomGenerator;
        private WorkItemClassificationContext _context;

        [SetUp]
        public void SetUp()
        {
            _randomGenerator = new RandomGenerator();
            var classifiers = new IWorkItemsClassifier[]
            {
                new DummyClassifier(DummyResolution1),
                new DummyClassifier(DummyResolution2),
                new DummyClassifier(type: Vsts.Constants.WorkItemTypeBug),
                new ExceptionWorkItemClassifier()
            };
            _context = new WorkItemClassificationContext(classifiers);

            _teamMembers = Builder<TeamMemberViewModel>.CreateListOfSize(5)
                .Build();
        }

        [Test]
        public void CallAllClassifier()
        {
            var result = Classify();

            result.Should().HaveCount(2);
            result.Should().NotContain(r => r.IsNone);
            result.Should().OnlyContain(r => r.Resolution == DummyResolution1 || r.Resolution == DummyResolution2);
        }

        [Test]
        public void PassCorrectClassificationRequest()
        {
            WorkItemResolutionRequest request = null;
            var c = new DummyClassifier(DummyResolution1, requestProcessor: r => request = r);
            var ctx = new WorkItemClassificationContext(new[] { c });

            var workitem = GetWorkitem(DummyClassifier.SupportedType);
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow.AddDays(-2);
            ctx.Classify(workitem, new ClassificationScope(_teamMembers, startDate, endDate));

            request.Should().NotBeNull();
            request.Team.Should().BeEquivalentTo(_teamMembers);
            request.WorkItem.Should().Be(workitem);
            request.StartDate.Should().Be(startDate);
            request.EndDate.Should().Be(endDate);
        }

        [Test]
        public void FilterOutEmptyClassificationResults()
        {
            var result = Classify(type: Vsts.Constants.WorkItemTypeBug);

            result.Should().HaveCount(1);
            result.Should().NotContain(r => r.IsNone);
            result.Should().OnlyContain(r => r.Resolution == DummyClassifier.ExpectedResolution);
        }

        [Test]
        public void ReturnResolutionsAsList()
        {
            var result = Classify();

            result.Should().BeOfType<List<WorkItemResolution>>();
        }

        [Test]
        public void NotFilterErrorClassificationResults()
        {
            var result = Classify(type: ExceptionWorkItemClassifier.SupportedType);

            result.Should().HaveCount(1);
            result.Should().OnlyContain(r => r.IsError);
        }

        [Test]
        public void FilterResultsByStartAndEndDate()
        {
            var workitem = GetWorkitem(DummyClassifier.SupportedType, resolvedOn: DateTime.UtcNow);
            var startDate = DateTime.UtcNow.AddDays(-3);
            var endDate = DateTime.UtcNow.AddDays(-2);
            var result = _context.Classify(workitem, new ClassificationScope(_teamMembers, startDate, endDate));

            result.Should().BeEmpty();
        }

        private IEnumerable<WorkItemResolution> Classify(string type = DummyClassifier.SupportedType)
        {
            var workitem = GetWorkitem(type, resolvedOn: DateTime.UtcNow.AddDays(-2));
            var startDate = DateTime.UtcNow.AddDays(-3);
            var endDate = DateTime.UtcNow;
            return _context.Classify(workitem, new ClassificationScope(_teamMembers, startDate, endDate));
        }

        private WorkItemViewModel GetWorkitem(string type, DateTime? resolvedOn = null)
        {
            var user = _teamMembers.First();
            var workitem = Builder<WorkItemViewModel>.CreateNew()
                .With(w => w.Fields = new Dictionary<string, string>())
                .With(w => w.Fields[Vsts.Constants.WorkItemTypeField] = type)
                .With(w => w.Fields[Vsts.Constants.WokItemCreatedDateField] = _randomGenerator.Next(DateTime.UtcNow.AddDays(-3), DateTime.UtcNow.AddDays(-1)).ToString())
                .Build();

            workitem.Updates = UpdateBuilder.Create()
                  .Activated().Then()
                  .Resolved(user).On(resolvedOn ?? DateTime.UtcNow)
                  .Then()
                  .Closed(user, reason: "Verified").On(DateTime.UtcNow)
                  .Build();

            return workitem;
        }
    }
}
