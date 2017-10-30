using Ether.Core.Configuration;
using Ether.Core.Constants;
using Ether.Core.Interfaces;
using Ether.Core.Models;
using Ether.Core.Models.DTO;
using Ether.Core.Models.DTO.Reports;
using Ether.Core.Models.VSTS;
using Ether.Core.Reporters;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ether.Tests.Reporters
{
    [TestFixture]
    public class WorkItemsReporterTests
    {
        private const int ActualMembersCount = 3;

        private Mock<IRepository> _repositoryMock;
        private Mock<ILogger<WorkItemsReporter>> _loggerMock;
        private Mock<IWorkItemsClassifier> _classifierMock;
        private WorkItemsReporter _reporter;
        private Profile _profile;
        private IEnumerable<TeamMember> _team;
        private Random _random = new Random(0);

        [SetUp]
        public void SetUp()
        {
            _repositoryMock = new Mock<IRepository>(MockBehavior.Strict);
            _classifierMock = new Mock<IWorkItemsClassifier>(MockBehavior.Strict);
            _loggerMock = new Mock<ILogger<WorkItemsReporter>>();
            var configMock = new Mock<IOptions<VSTSConfiguration>>();
            Common.SetupConfiguration(configMock);
            var data = Common.SetupDataForBaseReporter(_repositoryMock, membersCount: 4, takeMembers: ActualMembersCount);
            _profile = data.profile;
            _team = data.members;

            _reporter = new WorkItemsReporter(_repositoryMock.Object, configMock.Object, new[] { _classifierMock.Object }, _loggerMock.Object);
        }

        [Test]
        public async Task ShouldReturnEmptyReportIfNoData()
        {
            _repositoryMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<VSTSWorkItem, bool>>>()))
                .Returns(Task.FromResult(Enumerable.Empty<VSTSWorkItem>()))
                .Verifiable();

            var result = await _reporter.ReportAsync(new Core.Models.ReportQuery
            {
                ProfileId = _profile.Id
            });

            result.Should().NotBeNull();
            result.As<WorkItemsReport>()
                .Resolutions.Should().BeEmpty();
            result.As<WorkItemsReport>()
                .TotalResolved.Should().Be(0);
            result.As<WorkItemsReport>()
                .TotalInvestigated.Should().Be(0);

            _repositoryMock.Verify();
        }

        [Test]
        public async Task ShouldSelectOnlyWorkItemsForRequestedPeriod()
        {
            var utcNow = DateTime.UtcNow;
            var workitems = new[]
            {
                GetWorkItemWithDate(createdDate: null),
                GetWorkItemWithDate(utcNow.AddDays(-7)),
                GetWorkItemWithDate(utcNow.AddDays(-10)),
                GetWorkItemWithDate(createdDate: null),
                GetWorkItemWithDate(utcNow.AddDays(10)),
                GetWorkItemWithDate(utcNow.AddDays(5)),
                GetWorkItemWithDate(utcNow.AddDays(-5)),
                GetWorkItemWithDate(utcNow.AddDays(-1)),
                GetWorkItemWithDate(utcNow),
            };
            var expectedWorkItems = new[]
            {
                workitems[0], workitems[1], workitems[2], workitems[3], workitems[6], workitems[7]
            };

            _team.ElementAt(0).RelatedWorkItemIds = workitems.Select(w => w.WorkItemId);

            _repositoryMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<VSTSWorkItem, bool>>>()))
                .Returns<Expression<Func<VSTSWorkItem, bool>>>(e => Task.FromResult(workitems.Where(e.Compile())))
                .Verifiable();
            _classifierMock.Setup(c => c.Classify(It.IsAny<WorkItemResolutionRequest>()))
                .Returns<WorkItemResolutionRequest>(r => GetResolvedResolutionFor(r.WorkItem));

            var result = await _reporter.ReportAsync(new ReportQuery
            {
                StartDate = utcNow.AddDays(-7),
                EndDate = utcNow.AddDays(-1),
                ProfileId = _profile.Id
            });

            result.Should().NotBeNull();
            result.As<WorkItemsReport>()
                .Resolutions.Should().HaveCount(expectedWorkItems.Count());
            var workItemIds = result.As<WorkItemsReport>()
                .Resolutions
                .Select(r => r.WorkItemId);
            workItemIds.Should().BeEquivalentTo(expectedWorkItems.Select(w => w.WorkItemId));
            result.As<WorkItemsReport>()
                .TotalResolved.Should().Be(expectedWorkItems.Count());
            result.As<WorkItemsReport>()
                .TotalInvestigated.Should().Be(0);
            _repositoryMock.Verify();
        }

        [Test]
        public async Task ShouldSendACorrectResolutionRequest()
        {
            var utcNow = DateTime.UtcNow;
            var workitems = new[]
            {
                GetWorkItemWithDate(createdDate: null),
                GetWorkItemWithDate(createdDate: null),
                GetWorkItemWithDate(createdDate: null)
            };

            _team.ElementAt(0).RelatedWorkItemIds = workitems.Select(w => w.WorkItemId);

            _repositoryMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<VSTSWorkItem, bool>>>()))
                .Returns<Expression<Func<VSTSWorkItem, bool>>>(e => Task.FromResult(workitems.Where(e.Compile())))
                .Verifiable();
            _classifierMock.Setup(c => c.Classify(It.IsAny<WorkItemResolutionRequest>()))
                .Returns(WorkItemResolution.None);

            var result = await _reporter.ReportAsync(new ReportQuery
            {
                StartDate = utcNow.AddDays(-7),
                EndDate = utcNow.AddDays(-1),
                ProfileId = _profile.Id
            });

            _classifierMock.Verify(c => c.Classify(It.Is<WorkItemResolutionRequest>(r => IsValidResolutionRequest(r))), Times.Exactly(3));
            _repositoryMock.Verify();

            result.As<WorkItemsReport>()
                .Resolutions.Should().HaveCount(0);
            result.As<WorkItemsReport>()
                .TotalResolved.Should().Be(0);
            result.As<WorkItemsReport>()
                .TotalInvestigated.Should().Be(0);
        }

        [Test]
        public async Task ShouldTakeOnlyRelatedWorkItemsRequest()
        {
            var utcNow = DateTime.UtcNow;
            var workitems = new[]
            {
                GetWorkItemWithDate(createdDate: null),
                GetWorkItemWithDate(createdDate: null),
                GetWorkItemWithDate(createdDate: null),
                GetWorkItemWithDate(createdDate: null),
                GetWorkItemWithDate(createdDate: null),
                GetWorkItemWithDate(createdDate: null)
            };
            var expectedWorkItemsIds = new[] { workitems[0].WorkItemId, workitems[1].WorkItemId, workitems[4].WorkItemId, workitems[5].WorkItemId };

            _team.ElementAt(0).RelatedWorkItemIds = null;
            _team.ElementAt(1).RelatedWorkItemIds = new[] { workitems[0].WorkItemId, workitems[1].WorkItemId };
            _team.ElementAt(2).RelatedWorkItemIds = new[] { workitems[4].WorkItemId, workitems[5].WorkItemId };

            _repositoryMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<VSTSWorkItem, bool>>>()))
                .Returns<Expression<Func<VSTSWorkItem, bool>>>(e => Task.FromResult(workitems.Where(e.Compile())))
                .Verifiable();
            _classifierMock.Setup(c => c.Classify(It.IsAny<WorkItemResolutionRequest>()))
                .Returns<WorkItemResolutionRequest>(r => GetResolvedResolutionFor(r.WorkItem, GetMemberFromWorkItem(r.WorkItem)))
                .Verifiable();

            var result = await _reporter.ReportAsync(new ReportQuery
            {
                StartDate = utcNow.AddDays(-7),
                EndDate = utcNow.AddDays(-1),
                ProfileId = _profile.Id
            });

            _classifierMock.Verify();
            _repositoryMock.Verify();

            result.As<WorkItemsReport>()
                .Resolutions.Should().HaveCount(4);
            result.As<WorkItemsReport>()
                .Resolutions
                .Select(w=>w.WorkItemId)
                .ShouldBeEquivalentTo(expectedWorkItemsIds);
        }

        [Test]
        public async Task ShouldIgnoreNoneResolutions()
        {
            var utcNow = DateTime.UtcNow;
            var workitems = new[]
            {
                GetWorkItemWithDate(createdDate: null),
                GetWorkItemWithDate(createdDate: null),
                GetWorkItemWithDate(createdDate: null)
            }.AsEnumerable();

            var noneId = workitems.ElementAt(1).WorkItemId;

            _repositoryMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<VSTSWorkItem, bool>>>()))
                .Returns(Task.FromResult(workitems))
                .Verifiable();
            _classifierMock.Setup(c => c.Classify(It.IsAny<WorkItemResolutionRequest>()))
                .Returns<WorkItemResolutionRequest>(r => r.WorkItem.WorkItemId == noneId ? WorkItemResolution.None : GetResolvedResolutionFor(r.WorkItem, _team.ElementAt(0)))
                .Verifiable();

            var result = await _reporter.ReportAsync(new ReportQuery
            {
                StartDate = utcNow.AddDays(-7),
                EndDate = utcNow.AddDays(-1),
                ProfileId = _profile.Id
            });

            _classifierMock.Verify();
            _repositoryMock.Verify();

            result.As<WorkItemsReport>()
                .Resolutions.Should().HaveCount(2);
            result.As<WorkItemsReport>()
                .TotalResolved.Should().Be(2);
            result.As<WorkItemsReport>()
                .TotalInvestigated.Should().Be(0);
        }

        [Test]
        public async Task ShouldIgnoreWorkItemsThatAreNotInAQueryRange()
        {
            var utcNow = DateTime.UtcNow;
            var vstsMaxDate = DateTime.Parse("1/1/9999 12:00:00 AM");
            var workitems = new[]
            {
                GetWorkItemWithDate(createdDate: null),
                GetWorkItemWithDate(createdDate: null),
                GetWorkItemWithDate(createdDate: null)
            };

            _repositoryMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<VSTSWorkItem, bool>>>()))
                .Returns(Task.FromResult(workitems.AsEnumerable()))
                .Verifiable();
            _classifierMock.Setup(c => c.Classify(It.Is<WorkItemResolutionRequest>(w => w.WorkItem == workitems[0])))
                .Returns<WorkItemResolutionRequest>(r => GetResolvedResolutionFor(r.WorkItem, _team.ElementAt(0), DateTime.UtcNow.AddDays(-8)))
                .Verifiable();
            _classifierMock.Setup(c => c.Classify(It.Is<WorkItemResolutionRequest>(w => w.WorkItem == workitems[1])))
                .Returns<WorkItemResolutionRequest>(r => GetResolvedResolutionFor(r.WorkItem, _team.ElementAt(0), utcNow))
                .Verifiable();
            _classifierMock.Setup(c => c.Classify(It.Is<WorkItemResolutionRequest>(w => w.WorkItem == workitems[2])))
                .Returns<WorkItemResolutionRequest>(r => GetResolvedResolutionFor(r.WorkItem, _team.ElementAt(0), DateTime.UtcNow.AddDays(-2)))
                .Verifiable();

            var result = await _reporter.ReportAsync(new ReportQuery
            {
                StartDate = utcNow.AddDays(-7),
                EndDate = utcNow.AddDays(-1),
                ProfileId = _profile.Id
            });

            _classifierMock.Verify();
            _repositoryMock.Verify();

            result.As<WorkItemsReport>()
                .Resolutions.Should().HaveCount(1);
            result.As<WorkItemsReport>()
                .Resolutions.Select(r => r.WorkItemId)
                .ShouldBeEquivalentTo(new[] { workitems[2].WorkItemId });
            result.As<WorkItemsReport>()
                .TotalResolved.Should().Be(1);
            result.As<WorkItemsReport>()
                .TotalInvestigated.Should().Be(0);
        }

        [Test]
        public void ShouldReturnCorrectMetadata()
        {
            _reporter.Id.Should().Be(Guid.Parse("54c62ebe-cfef-46d5-b90f-ebb00a1611b7"));
            _reporter.Name.Should().Be("Work items report");
            _reporter.ReportType.Should().Be(typeof(WorkItemsReport));
        }

        private WorkItemResolution GetResolvedResolutionFor(VSTSWorkItem workItem, TeamMember member = null, DateTime? revisedDate = null)
        {
            var date = revisedDate ?? DateTime.UtcNow.AddDays(-2);
            var teamMember = member ?? new TeamMember { Id = Guid.NewGuid(), DisplayName = Guid.NewGuid().ToString(), Email = Guid.NewGuid().ToString() };
            return new WorkItemResolution(workItem, "Resolved", "Because", date, teamMember.Email, teamMember.DisplayName);
        }

        private VSTSWorkItem GetWorkItemWithDate(DateTime? createdDate)
        {
            var wi = new VSTSWorkItem { Fields = new Dictionary<string, string>() };
            wi.Id = Guid.NewGuid();
            wi.WorkItemId = _random.Next(100_000_000);
            if (createdDate.HasValue)
                wi.Fields.Add(VSTSFieldNames.WorkItemCreatedDate, createdDate.Value.ToString("s"));

            return wi;
        }

        private bool IsValidResolutionRequest(WorkItemResolutionRequest request)
        {
            return request.WorkItem != null && request.Team != null && request.Team.Count() == ActualMembersCount;
        }

        private TeamMember GetMemberFromWorkItem(VSTSWorkItem workItem)
        {
            return _team.SingleOrDefault(m => m.RelatedWorkItemIds!=null &&  m.RelatedWorkItemIds.Contains(workItem.WorkItemId));
        }
    }
}
