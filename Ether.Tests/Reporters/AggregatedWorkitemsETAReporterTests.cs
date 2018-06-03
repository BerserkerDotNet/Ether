using Ether.Core.Constants;
using Ether.Core.Interfaces;
using Ether.Core.Models;
using Ether.Core.Models.DTO;
using Ether.Core.Models.DTO.Reports;
using Ether.Core.Models.VSTS;
using Ether.Core.Reporters;
using Ether.Core.Types;
using Ether.Core.Types.Exceptions;
using Ether.Tests.Data;
using Ether.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Logging;
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
    public class AggregatedWorkitemsETAReporterTests : BaseTest
    {
        private const string OriginalEstimateField = "OE";
        private const string CompletedWorkField = "CW";
        private const string RemainingWorkField = "RW";

        private Random _random = new Random();
        private Mock<ILogger<AggregatedWorkitemsETAReporter>> _loggerMock;

        private Mock<IWorkItemClassificationContext> _classificationContextMock = new Mock<IWorkItemClassificationContext>();
        private AggregatedWorkitemsETAReporter _reporter;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _loggerMock = new Mock<ILogger<AggregatedWorkitemsETAReporter>>();
            _reporter = new AggregatedWorkitemsETAReporter(Data.RepositoryMock.Object, 
                _classificationContextMock.Object, 
                Data.ConfigurationMock.Object,
                Mock.Of<IProgressReporter>(),
                _loggerMock.Object);
        }

        protected override void ProcessData(TestData data)
        {
            var defaultETASettings = new Settings { WorkItemsSettings = new Settings.WorkItems() { ETAFields = ETAFields } };
            data.RepositoryMock.Setup(r => r.GetSingleAsync<Settings>(It.IsAny<Expression<Func<Settings, bool>>>()))
                    .ReturnsAsync(defaultETASettings);
        }

        [Test]
        [TestData(membersCount: 2, repositoryCount: 2, RelatedWorkItemsPerMember = 10)]
        public async Task ShouldFetchWorkitemsForMembersInProfile()
        {
            // TODO: Revisit this test
            var allMembers = Data.TeamMembers
                .Union(Data.CreateTeamMembers(2));

            var workitems = allMembers.Select(m =>
            {
                var workItemId = Data.TeamMemberIds.Contains(m.Id) ? m.RelatedWorkItemIds.Random() : -_random.Next(0, 50);
                var workItem = new VSTSWorkItem { Id = Guid.NewGuid(), WorkItemId = workItemId, Fields = new Dictionary<string, string>() };
                workItem.Fields.Add(VSTSFieldNames.WorkItemType, WorkItemTypes.Bug);
                workItem.Fields.Add(VSTSFieldNames.WorkItemCreatedDate, DateTime.UtcNow.AddDays(-5).ToString());
                workItem.Updates = UpdateBuilder.Create()
                    .Activated()
                    .Then().Resolved(m)
                    .Build();

                return workItem;
            }).ToList();
            _classificationContextMock.Setup(c => c.Classify(It.IsAny<VSTSWorkItem>(), It.IsAny<ClassificationScope>()))
                    .Returns<VSTSWorkItem, ClassificationScope>((w, _) =>
                    {
                        var member = Data.TeamMembers.Single(t => t.RelatedWorkItemIds.Contains(w.WorkItemId));
                        return new[]
                        {
                            new WorkItemResolution(w, WorkItemStates.Resolved, "Because", DateTime.UtcNow, member.Email, "bla")
                        };
                    });
            Data.RepositoryMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<VSTSWorkItem, bool>>>()))
                .Returns<Expression<Func<VSTSWorkItem, bool>>>(e => Task.FromResult(workitems.Where(e.Compile())));

            var report = await Report();

            report.Should().NotBeNull();
            report.Should().BeOfType<AggregatedWorkitemsETAReport>();
            report.IndividualReports.Should().HaveCount(2);
            report.IndividualReports.Should().OnlyContain(r => Data.TeamMembers.Any(m => m.Email == r.MemberEmail && m.DisplayName == r.MemberName));
            report.IndividualReports.Should().OnlyContain(r => r.OriginalEstimated == 0);
            report.IndividualReports.Should().OnlyContain(r => r.CompletedWithEstimates == 0);
            report.IndividualReports.Should().OnlyContain(r => r.WithoutETA == 1);
            report.IndividualReports.Should().OnlyContain(r => r.TotalResolved == 1);
        }

        [TestCase(WorkItemTypes.Bug, WorkItemStates.Resolved, 1)]
        [TestCase(WorkItemTypes.Bug, WorkItemStates.Closed, 0)]
        [TestCase(WorkItemTypes.Task, WorkItemStates.Resolved, 1)]
        [TestCase(WorkItemTypes.Task, WorkItemStates.Closed, 1)]
        [TestData(membersCount: 1, repositoryCount: 2, RelatedWorkItemsPerMember = 1)]
        public async Task ShouldCorrectlyCalculateResolvedCount(string workItemType, string resolution, int expectedCount)
        {
            var workitems = Data.TeamMembers.Select(m =>
            {
                var workItemId = m.RelatedWorkItemIds.Random();
                var workItem = new VSTSWorkItem { Id = Guid.NewGuid(), WorkItemId = workItemId, Fields = new Dictionary<string, string>() };
                workItem.Fields.Add(VSTSFieldNames.WorkItemType, workItemType);
                workItem.Fields.Add(VSTSFieldNames.WorkItemCreatedDate, DateTime.UtcNow.AddDays(-5).ToString());

                return workItem;
            }).ToList();
            _classificationContextMock.Setup(c => c.Classify(It.IsAny<VSTSWorkItem>(), It.IsAny<ClassificationScope>()))
                    .Returns<VSTSWorkItem, ClassificationScope>((w, _) =>
                    {
                        var member = Data.TeamMembers.Single(t => t.RelatedWorkItemIds.Contains(w.WorkItemId));
                        return new[]
                        {
                            new WorkItemResolution(w, resolution, "Because", DateTime.UtcNow, member.Email, "bla")
                        };
                    });
            Data.RepositoryMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<VSTSWorkItem, bool>>>()))
                .Returns<Expression<Func<VSTSWorkItem, bool>>>(e => Task.FromResult(workitems.Where(e.Compile())));

            var report = await Report();

            report.TotalResolved.Should().Be(expectedCount);
        }

        [Test]
        [TestData(membersCount: 1, repositoryCount: 2, RelatedWorkItemsPerMember = 1)]
        public async Task ShouldReturnEMptyIndividualReportIfNoResolutions()
        {
            var workitems = Data.TeamMembers.Select(m =>
            {
                var workItemId = m.RelatedWorkItemIds.Random();
                var workItem = new VSTSWorkItem { Id = Guid.NewGuid(), WorkItemId = workItemId, Fields = new Dictionary<string, string>() };
                workItem.Fields.Add(VSTSFieldNames.WorkItemType, WorkItemTypes.Bug);
                workItem.Fields.Add(VSTSFieldNames.WorkItemCreatedDate, DateTime.UtcNow.AddDays(-5).ToString());

                return workItem;
            }).ToList();
            _classificationContextMock.Setup(c => c.Classify(It.IsAny<VSTSWorkItem>(), It.IsAny<ClassificationScope>()))
                    .Returns(Enumerable.Empty<WorkItemResolution>());
            Data.RepositoryMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<VSTSWorkItem, bool>>>()))
                .Returns<Expression<Func<VSTSWorkItem, bool>>>(e => Task.FromResult(workitems.Where(e.Compile())));

            var report = await Report();

            var firstMember = Data.TeamMembers.ElementAt(0);
            report.IndividualReports.Should().HaveCount(1);
            report.IndividualReports.Should().OnlyContain(r => r.MemberEmail == firstMember.Email &&
            r.MemberName == firstMember.DisplayName &&
            r.TotalResolved == 0);
        }

        [Test]
        [TestData(membersCount: 2, repositoryCount: 2, RelatedWorkItemsPerMember = 10)]
        public async Task ShouldReturnEmptyReportIfNoWorkItems()
        {
            Data.RepositoryMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<VSTSWorkItem, bool>>>()))
                .ReturnsAsync(Enumerable.Empty<VSTSWorkItem>());
            await CheckForEmptyReport();
        }

        [Test]
        [TestData(membersCount: 2, repositoryCount: 1, projectsCount: 1)]
        public void ShouldThrowIfNoETASettings()
        {
            var emptySettings = new Settings { WorkItemsSettings = new Settings.WorkItems() { ETAFields = Enumerable.Empty<Settings.Field>() } };
            Data.RepositoryMock.Setup(r => r.GetSingleAsync(It.IsAny<Expression<Func<Settings, bool>>>())).ReturnsAsync(emptySettings);

            var report = _reporter.Awaiting(r => r.ReportAsync(Data.GetDefaultQuery()))
                .Should().Throw<MissingETASettingsException>();
        }

        [Test]
        [TestData(membersCount: 2, repositoryCount: 1, projectsCount: 1)]
        public void ShouldThrowIfETASettingsAreNull()
        {
            var emptySettings = new Settings { WorkItemsSettings = new Settings.WorkItems() { ETAFields = null } };
            Data.RepositoryMock.Setup(r => r.GetSingleAsync(It.IsAny<Expression<Func<Settings, bool>>>())).ReturnsAsync(emptySettings);

            var report = _reporter.Awaiting(r => r.ReportAsync(Data.GetDefaultQuery()))
                .Should().Throw<MissingETASettingsException>();
        }

        [Test]
        [TestData(membersCount: 2, repositoryCount: 0, projectsCount: 1)]
        public async Task ShouldReturnEmptyReportIfNoRepositories()
        {
            await CheckForEmptyReport();
        }

        [Test]
        [TestData(membersCount: 0, repositoryCount: 2, projectsCount: 1)]
        public async Task ShouldReturnEmptyReportIfNoMembers()
        {
            await CheckForEmptyReport();
        }

        [Test]
        [TestData(membersCount: 2, repositoryCount: 2, projectsCount: 1, RelatedWorkItemsPerMember = 2)]
        public async Task ShouldUseClassifiersToDetermineTheTotalResolvedCount()
        {
            var report = await ExecuteReportWithResolutions();

            report.IndividualReports.Should().HaveCount(2);
            report.IndividualReports.Should().OnlyContain(r => r.TotalResolved == 2);
        }

        [Test]
        [TestData(membersCount: 2, repositoryCount: 2, projectsCount: 1, RelatedWorkItemsPerMember = 2)]
        public async Task ShouldIncrementWithoutETACountIfZeroETA()
        {
            var report = await ExecuteReportWithResolutions();

            report.IndividualReports.Should().HaveCount(2);
            report.IndividualReports.Should().OnlyContain(r => r.WithoutETA == 2);
        }

        [TestCase(1, 2, 4, 4)]
        [TestCase(1, 2, null, 0)]
        [TestCase(1, null, null, 0)]
        [TestCase(null, 2, null, 0)]
        [TestCase(null, null, 4, 4)]
        [TestCase(null, 2, 4, 4)]
        [TestCase(null, null, null, 0)]
        [TestData(membersCount: 1, repositoryCount: 2, projectsCount: 1, RelatedWorkItemsPerMember = 1)]
        public async Task ShouldCalculateTotalPlannedETA(int? completed, int? remaining, int? original, int expected)
        {
            var report = await ExecuteReportWithResolutions(completed, remaining, expected);

            report.IndividualReports.Should().HaveCount(1);
            report.IndividualReports.Should().OnlyContain(r => r.OriginalEstimated == expected);
        }


        [TestCase(1, 2, 4, 3)]
        [TestCase(1, 2, null, 3)]
        [TestCase(1, null, null, 1)]
        [TestCase(null, 2, null, 2)]
        [TestCase(null, null, 4, 4)]
        [TestCase(null, 2, 4, 2)]
        [TestCase(null, null, null, 0)]
        [TestData(membersCount: 1, repositoryCount: 2, projectsCount: 1, RelatedWorkItemsPerMember = 1)]
        public async Task ShouldCalculateTotalCompletedETA(int? completed, int? remaining, int? original, int expected)
        {
            var report = await ExecuteReportWithResolutions(completed, remaining, expected);

            report.IndividualReports.Should().HaveCount(1);
            report.IndividualReports.Should().OnlyContain(r => r.EstimatedToComplete == expected);
        }

        [Test, TestCaseSource(typeof(ETADataProvider), nameof(ETADataProvider.WorkItemActiveTimeTestsWithETA))]
        [TestData(membersCount: 0, RegisterDummyMember = true, RelatedWorkItemsPerMember = 2)]
        public async Task ShouldCorrectlyIdentifyActiveTimeWithEstimates(IEnumerable<WorkItemUpdate> updates, float expectedDuration)
        {
            var report = await ExecuteReportWithResolutions(original: 1, updates: updates);

            report.IndividualReports.Should().HaveCount(1);
            report.IndividualReports.Should().OnlyContain(r => r.CompletedWithEstimates == expectedDuration*2);
        }

        [Test, TestCaseSource(typeof(ETADataProvider), nameof(ETADataProvider.WorkItemActiveTimeTestsNoETA))]
        [TestData(membersCount: 0, RegisterDummyMember = true, RelatedWorkItemsPerMember = 2)]
        public async Task ShouldCorrectlyIdentifyActiveTimeWithoutEstimates(IEnumerable<WorkItemUpdate> updates, float expectedDuration)
        {
            var report = await ExecuteReportWithResolutions(updates: updates);

            report.IndividualReports.Should().HaveCount(1);
            report.IndividualReports.Should().OnlyContain(r => r.CompletedWithoutEstimates == expectedDuration * 2);
        }

        private async Task<AggregatedWorkitemsETAReport> Report()
        {
            return await _reporter.ReportAsync(Data.GetDefaultQuery()) as AggregatedWorkitemsETAReport;
        }

        private async Task CheckForEmptyReport()
        {
            var report = await Report();

            report.Should().NotBeNull();
            report.Should().BeOfType<AggregatedWorkitemsETAReport>();
            report.As<AggregatedWorkitemsETAReport>().IndividualReports.Should().BeEmpty();
        }


        private async Task<AggregatedWorkitemsETAReport> ExecuteReportWithResolutions(int? completed = null, int? remaining = null, int? original = null, IEnumerable<WorkItemUpdate> updates = null)
        {
            var workitems = Data.TeamMembers
                .SelectMany(t => t.RelatedWorkItemIds)
                .Select(id =>
                {
                    var wi = new VSTSWorkItem
                    {
                        WorkItemId = id,
                        Fields = new Dictionary<string, string>()
                    };
                    wi.Fields[VSTSFieldNames.WorkItemType] = WorkItemTypes.Bug;
                    wi.Fields[VSTSFieldNames.Title] = "Bla";
                    if (completed.HasValue)
                        wi.Fields[CompletedWorkField] = completed.ToString();
                    if (remaining.HasValue)
                        wi.Fields[RemainingWorkField] = remaining?.ToString();
                    if (original.HasValue)
                        wi.Fields[OriginalEstimateField] = original?.ToString();
                    if (updates != null)
                        wi.Updates = updates;
                    return wi;
                }).ToList();

            Data.RepositoryMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<VSTSWorkItem, bool>>>()))
                .Returns<Expression<Func<VSTSWorkItem, bool>>>(e => Task.FromResult(workitems.Where(e.Compile())));
            _classificationContextMock.Setup(c => c.Classify(It.IsAny<VSTSWorkItem>(), It.IsAny<ClassificationScope>()))
                .Returns<VSTSWorkItem, ClassificationScope>((w, _) =>
                {
                    var member = Data.TeamMembers.Single(t => t.RelatedWorkItemIds.Contains(w.WorkItemId));
                    return new[]
                    {
                        new WorkItemResolution(w, WorkItemStates.Resolved, "Because", DateTime.UtcNow, member.Email, "bla")
                    };
                });

            return await Report();
        }

        private static string FieldNameFor(string workItemType, ETAFieldType fieldType) => ETAFields.First(f => f.WorkitemType == workItemType && f.FieldType == fieldType).FieldName;

        private static IEnumerable<Settings.Field> ETAFields = new[] {
                new Settings.Field(WorkItemTypes.Bug, OriginalEstimateField, ETAFieldType.OriginalEstimate),
                new Settings.Field(WorkItemTypes.Bug, CompletedWorkField, ETAFieldType.CompletedWork),
                new Settings.Field(WorkItemTypes.Bug, RemainingWorkField, ETAFieldType.RemainingWork),
                new Settings.Field(WorkItemTypes.Task, OriginalEstimateField, ETAFieldType.OriginalEstimate),
                new Settings.Field(WorkItemTypes.Task, CompletedWorkField, ETAFieldType.CompletedWork),
                new Settings.Field(WorkItemTypes.Task, RemainingWorkField, ETAFieldType.RemainingWork),
            };
    }
}
