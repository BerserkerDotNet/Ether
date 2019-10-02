using System;
using System.Collections.Generic;
using System.Linq;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Types;
using Ether.ViewModels;
using Ether.Vsts;
using Ether.Vsts.Types.Classifiers;
using FizzWare.NBuilder;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
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
            _classifier = new ReOpenedWorkitemClassifier(Mock.Of<ILogger<ReOpenedWorkitemClassifier>>());
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
            var joe = SetupMembers("Joe", "Foo");
            var bug = SetupBug(updatesConfig =>
            {
                updatesConfig.New()
                    .Then().Activated()
                    .Then().AssignedTo(joe)
                    .Then().Resolved(joe).WithAssignedTo(old: joe, @new: null)
                    .Then().Activated(from: "Resolved").On(expectedReopenDate);
            });

            var events = Run(bug, joe);

            events.Should().HaveCount(1);
            Verify(events.First(), bug, joe, expectedReopenDate);
        }

        [Test]
        public void NoReOpens()
        {
            var joe = SetupMembers("Joe", "Foo");
            var bug = SetupBug(updatesConfig =>
            {
                updatesConfig.New()
                    .Then().Activated()
                    .Then().AssignedTo(joe)
                    .Then().Resolved(joe).WithAssignedTo(old: joe, @new: null)
                    .Then().Closed(from: "Resolved");
            });

            var events = Run(bug, joe);

            events.Should().HaveCount(0);
        }

        [Test]
        public void OneReOpenIsNotAfterResolvedByTeam()
        {
            var joe = SetupMembers("Joe", "Foo");
            var mary = SetupMembers("Mary", "Foo");
            var bug = SetupBug(updatesConfig =>
            {
                updatesConfig.New()
                    .Then().Activated()
                    .Then().AssignedTo(mary)
                    .Then().Resolved(mary).WithAssignedTo(old: mary, @new: null)
                    .Then().Activated(from: "Resolved");
            });

            var events = Run(bug, joe);

            events.Should().HaveCount(0);
        }

        [Test]
        public void OneReOpenIsNotAfterResolvedByTeamAndOneIs()
        {
            var expectedReopenDate = DateTime.UtcNow.AddDays(5);
            var joe = SetupMembers("Joe", "Foo");
            var mary = SetupMembers("Mary", "Foo");
            var bug = SetupBug(updatesConfig =>
            {
                updatesConfig.New()
                    .Then().Activated()
                    .Then().AssignedTo(mary)
                    .Then().Resolved(mary).WithAssignedTo(old: mary, @new: null)
                    .Then().Activated(from: "Resolved")
                    .Then().AssignedTo(joe)
                    .Then().Resolved(joe)
                    .Then().Activated(from: "Resolved").On(expectedReopenDate);
            });

            var events = Run(bug, joe);

            events.Should().HaveCount(1);
            Verify(events.First(), bug, joe, expectedReopenDate);
        }

        [Test]
        public void MultipleReOpens()
        {
            var expectedReopenDateMary = DateTime.UtcNow.AddDays(3);
            var expectedReopenDateJoe = DateTime.UtcNow.AddDays(5);
            var joe = SetupMembers("Joe", "Foo");
            var mary = SetupMembers("Mary", "Foo");
            var bug = SetupBug(updatesConfig =>
            {
                updatesConfig.New()
                    .Then().Activated()
                    .Then().AssignedTo(mary)
                    .Then().Resolved(mary).WithAssignedTo(old: mary, @new: null)
                    .Then().Activated(from: "Resolved").On(expectedReopenDateMary)
                    .Then().AssignedTo(joe)
                    .Then().Resolved(joe)
                    .Then().Activated(from: "Resolved").On(expectedReopenDateJoe);
            });

            var events = Run(bug, joe, mary);

            events.Should().HaveCount(2);
            Verify(events.First(), bug, mary, expectedReopenDateMary);
            Verify(events.Last(), bug, joe, expectedReopenDateJoe);
        }

        [Test]
        public void ReOpenFromClosed()
        {
            var expectedReopenDateJoe = DateTime.UtcNow.AddDays(5);
            var joe = SetupMembers("Joe", "Foo");
            var bug = SetupBug(updatesConfig =>
            {
                updatesConfig.New()
                    .Then().Activated()
                    .Then().AssignedTo(joe)
                    .Then().Resolved(joe).WithAssignedTo(old: joe, @new: null)
                    .Then().Closed(from: "Resolved")
                    .Then().Activated(from: "Closed").On(expectedReopenDateJoe);
            });

            var events = Run(bug, joe);

            events.Should().HaveCount(1);
            Verify(events.First(), bug, joe, expectedReopenDateJoe);
        }

        [Test]
        public void AssignedToDidNotChangeOnResolve()
        {
            var expectedReopenDateJoe = DateTime.UtcNow.AddDays(5);
            var joe = SetupMembers("Joe", "Foo");
            var bug = SetupBug(updatesConfig =>
            {
                updatesConfig.New()
                    .Then().Activated()
                    .Then().AssignedTo(joe)
                    .Then().Resolved(joe)
                    .Then().Activated(from: "Resolved").On(expectedReopenDateJoe);
            });

            var events = Run(bug, joe);

            events.Should().HaveCount(1);
            Verify(events.First(), bug, joe, expectedReopenDateJoe);
        }

        [Test]
        public void NoResolves()
        {
            var joe = SetupMembers("Joe", "Foo");
            var mary = SetupMembers("Mary", "Foo");
            var bug = SetupBug(updatesConfig =>
            {
                updatesConfig.New()
                    .Then().Activated()
                    .Then().AssignedTo(mary)
                    .Then().New()
                    .Then().AssignedTo(joe)
                    .Then().New();
            });

            var events = Run(bug, joe);

            events.Should().HaveCount(0);
        }

        [Test]
        public void ReOpenedFromClosedDirectly()
        {
            var expectedReopenDateJoe = DateTime.UtcNow.AddDays(5);
            var joe = SetupMembers("Joe", "Foo");
            var bug = SetupBug(updatesConfig =>
            {
                updatesConfig.New()
                    .Then().Activated()
                    .Then().AssignedTo(joe)
                    .Then().Closed(joe)
                    .Then().Activated(from: "Closed").On(expectedReopenDateJoe);
            });

            var events = Run(bug, joe);

            events.Should().HaveCount(1);
            Verify(events.First(), bug, joe, expectedReopenDateJoe);
        }

        [Test]
        public void ShouldSkipIfNoCorrespondingResolveUpdate()
        {
        }

        private IEnumerable<IWorkItemEvent> Run(WorkItemViewModel item, params TeamMemberViewModel[] team)
        {
            return _classifier.Classify(new WorkItemResolutionRequest
            {
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                WorkItem = item,
                Team = team
            });
        }

        private void Verify(IWorkItemEvent @event, WorkItemViewModel workItem, TeamMemberViewModel teamMember, DateTime expecetdDate)
        {
            @event.Should().BeOfType<WorkItemReOpenedEvent>();
            @event.Date.Should().BeCloseTo(expecetdDate, TimeSpan.FromSeconds(1));
            @event.WorkItem.Id.Should().Be(workItem.WorkItemId);
            @event.AssociatedUser.Should().NotBeNull();
            @event.AssociatedUser.Email.Should().Be(teamMember.Email);
            @event.AssociatedUser.Title.Should().Be(teamMember.DisplayName);
        }

        private WorkItemViewModel SetupBug(Action<UpdateBuilder> updatesConfig = null)
        {
            var bugUpdatesBuilder = UpdateBuilder.Create();
            updatesConfig?.Invoke(bugUpdatesBuilder);
            var bugUpdates = bugUpdatesBuilder.Build();

            var bugFields = new Dictionary<string, string> { { Constants.WorkItemTypeField, Constants.WorkItemTypeBug } };

            var bug = Builder<WorkItemViewModel>.CreateNew()
                .With(w => w.Fields, bugFields)
                .With(w => w.Updates, bugUpdates)
                .Build();

            return bug;
        }

        private TeamMemberViewModel SetupMembers(string firstName, string lastName)
        {
            return Builder<TeamMemberViewModel>.CreateNew()
                .With(m => m.DisplayName, $"{firstName} {lastName}")
                .With(m => m.Email, $"{firstName}.{lastName}@bla.com".ToLower())
                .Build();
        }
    }
}
