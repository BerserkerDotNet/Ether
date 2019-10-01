using System;
using System.Collections.Generic;
using System.Linq;
using Ether.Contracts.Types;
using Ether.ViewModels;
using Ether.Vsts;
using Ether.Vsts.Types.Classifiers;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;

namespace Ether.Tests.Classifiers
{
    [TestFixture]
    public class ReOpenedWorkitemClassifierTests
    {
        private ReOpenedWorkitemClassifier _classifier;

        [SetUp]
        public void SetUp()
        {
            _classifier = new ReOpenedWorkitemClassifier();
        }

        [Test]
        public void ShouldSupportOnlyBugs()
        {
            // _classifier.
        }

        [Test]
        public void SimpleCase()
        {
            var expectedReopenDate = DateTime.UtcNow;
            var joe = Builder<TeamMemberViewModel>.CreateNew()
                .With(m => m.DisplayName, "Joe Foo")
                .With(m => m.Email, "joe.foo@bla.com")
                .Build();

            var bugUpdates = UpdateBuilder.Create()
                .New()
                .Then().Activated()
                .Then().AssignedTo(joe)
                .Then().Resolved(joe).WithAssignedTo(old: joe, @new: null)
                .Then().Activated(from: "Resolved").On(expectedReopenDate)
                .Build();

            var bugFields = new Dictionary<string, string> { { Constants.WorkItemTypeField, Constants.WorkItemTypeBug } };

            var bug = Builder<WorkItemViewModel>.CreateNew()
                .With(w => w.Fields, bugFields)
                .With(w => w.Updates, bugUpdates)
                .Build();

            var events = _classifier.Classify(new WorkItemResolutionRequest
            {
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                WorkItem = bug,
                Team = new[] { joe }
            });

            events.Should().HaveCount(1);
            var @event = events.First();
            @event.Should().BeOfType<WorkItemReOpenedEvent>();
            @event.Date.Should().BeCloseTo(expectedReopenDate, TimeSpan.FromSeconds(1));
            @event.WorkItem.Id.Should().Be(bug.WorkItemId);
            @event.AssociatedUser.Should().NotBeNull();
            @event.AssociatedUser.Email.Should().Be(joe.Email);
            @event.AssociatedUser.Title.Should().Be(joe.DisplayName);
        }

        [Test]
        public void NoReOpenes()
        {
        }

        [Test]
        public void NoReOpeneIsNotAfterResolvedByTeam()
        {
        }

        [Test]
        public void OneReOpeneIsNotAfterResolvedByTeamAndOneIs()
        {
        }

        [Test]
        public void MultipleReOpenes()
        {
        }

        [Test]
        public void AssignedToDidnotChangeOnResolve()
        {
        }

        [Test]
        public void NoResolves()
        {
        }

        [Test]
        public void ResolveToClosed()
        {
        }
    }
}
