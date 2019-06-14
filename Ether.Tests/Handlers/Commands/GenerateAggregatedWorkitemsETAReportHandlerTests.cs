using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ether.Contracts.Dto;
using Ether.Contracts.Dto.Reports;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Types;
using Ether.Core.Types.Commands;
using Ether.Core.Types.Handlers.Commands;
using Ether.Tests.TestData;
using Ether.ViewModels;
using Ether.Vsts;
using Ether.Vsts.Types;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using static Ether.Tests.TestData.WorkItemsFactory;

namespace Ether.Tests.Handlers.Commands
{
    [TestFixture]
    public class GenerateAggregatedWorkitemsETAReportHandlerTests : BaseReportHandlerTests<GenerateAggregatedWorkitemsETAReportHandler, GenerateAggregatedWorkitemsETAReport>
    {
        private Mock<IWorkItemClassificationContext> _classificationContextMock;

        protected override string ReportType => Core.Types.Constants.ETAReportType;

        protected override string ReportName => Core.Types.Constants.ETAReportName;

        [Test]
        public void ShouldThrowExceptionIfNotSupportedDataSourceType([Values(null, "", "Bla")]string dataSourceType)
        {
            RepositoryMock.Setup(r => r.GetFieldValueAsync(It.IsAny<Expression<Func<Profile, bool>>>(), It.IsAny<Expression<Func<Profile, string>>>()))
                .ReturnsAsync(dataSourceType)
                .Verifiable();

            IDataSource ds;
            DataSourceProviderMock.Setup(p => p.TryGetValue(dataSourceType, out ds)).Returns(false);
            Handler.Awaiting(h => h.Handle(new GenerateAggregatedWorkitemsETAReport()))
                .Should().Throw<ArgumentException>();

            RepositoryMock.Verify();
        }

        [Test]
        public void ShouldThrowExceptionIfProfileNotFound()
        {
            SetupGetProfile(null);

            Handler.Awaiting(h => h.Handle(Command))
                .Should().Throw<ArgumentException>();

            DataSourceMock.Verify();
        }

        [Test]
        public async Task ShouldReturnEmptyReportForEachMemberIfNoWorkItems()
        {
            var profile = Builder<ProfileViewModel>.CreateNew()
                .With(p => p.Members, new Guid[0])
                .Build();

            SetupGetProfile(profile);
            DataSourceMock.Setup(d => d.GetWorkItemsFor(It.IsAny<Guid>()))
                .ReturnsAsync(Enumerable.Empty<WorkItemViewModel>())
                .Verifiable();

            await InvokeAndVerify<AggregatedWorkitemsETAReport>(Command, (report, reportId) =>
            {
                report.IndividualReports.Should().BeEmpty();
            });
        }

        [Test]
        public async Task ShouldReturnCorrectReportWithValidData()
        {
            var jim = Builder<TeamMemberViewModel>.CreateNew()
                .Build();
            var profile = Builder<ProfileViewModel>.CreateNew()
                .With(p => p.Members, new[] { jim.Id })
                .Build();

            SetupGetProfile(profile);
            SetupGetTeamMember(new[] { jim });
            var expectedValuesForJim = SetupJim(jim);

            await InvokeAndVerify<AggregatedWorkitemsETAReport>(Command, (report, reportId) =>
            {
                report.IndividualReports.Should().HaveCount(1);
                VerifyReportFor(jim, report, expectedValuesForJim);
            });
        }

        [Test]
        public async Task ShouldGenerateReportForMultipleMembers()
        {
            var members = Builder<TeamMemberViewModel>.CreateListOfSize(2)
                .Build();
            var profile = Builder<ProfileViewModel>.CreateNew()
                .With(p => p.Members, members.Select(m => m.Id))
                .Build();

            SetupGetProfile(profile);
            SetupGetTeamMember(members);

            var jim = members[0];
            var bob = members[1];
            var expectedValuesForJim = SetupJim(jim);
            var expectedValuesForBob = SetupBob(bob);

            await InvokeAndVerify<AggregatedWorkitemsETAReport>(Command, (report, reportId) =>
            {
                report.IndividualReports.Should().HaveCount(2);
                VerifyReportFor(jim, report, expectedValuesForJim);
                VerifyReportFor(bob, report, expectedValuesForBob);
            });
        }

        [Test]
        public async Task ShouldCountItemsWithoutETA()
        {
            var emmy = Builder<TeamMemberViewModel>.CreateNew()
                .Build();
            var profile = Builder<ProfileViewModel>.CreateNew()
                .With(p => p.Members, new[] { emmy.Id })
                .Build();

            SetupGetProfile(profile);
            SetupGetTeamMember(new[] { emmy });
            var workitemsForEmmy = new WorkItemTestDataContainer(new[]
            {
                CreateBug().WithNormalLifecycle(emmy, 3).WithETA(0, 5, 0),
                CreateTask().WithNormalLifecycle(emmy, 1),
                CreateBug().WithNormalLifecycle(emmy, 2),
                CreateTask().WithNormalLifecycle(emmy, 5),
                CreateBug().WithNormalLifecycle(emmy, 4).WithETA(0, 7, 0)
            });
            SetupGetWorkitems(emmy.Id, workitemsForEmmy.WorkItems);
            SetupClassify(workitemsForEmmy);

            await InvokeAndVerify<AggregatedWorkitemsETAReport>(Command, (report, reportId) =>
            {
                report.IndividualReports.Should().HaveCount(1);
                var individualReport = report.IndividualReports[0];
                individualReport.WithoutETA.Should().Be(3);
            });
        }

        [Test]
        public async Task ShouldCountTotalResolvedBugsAndTasks()
        {
            var emmy = Builder<TeamMemberViewModel>.CreateNew()
                .Build();
            var profile = Builder<ProfileViewModel>.CreateNew()
                .With(p => p.Members, new[] { emmy.Id })
                .Build();

            SetupGetProfile(profile);
            SetupGetTeamMember(new[] { emmy });
            var workitemsForEmmy = new WorkItemTestDataContainer(new[]
            {
                CreateBug().WithNormalLifecycle(emmy, 3).WithETA(0, 5, 0),
                CreateTask().WithNormalLifecycle(emmy, 1),
                CreateBug().WithNormalLifecycle(emmy, 2),
                CreateTask().WithNormalLifecycle(emmy, 5),
                CreateBug().WithNormalLifecycle(emmy, 4).WithETA(0, 7, 0)
            });
            SetupGetWorkitems(emmy.Id, workitemsForEmmy.WorkItems);
            SetupClassify(workitemsForEmmy);

            await InvokeAndVerify<AggregatedWorkitemsETAReport>(Command, (report, reportId) =>
            {
                report.IndividualReports.Should().HaveCount(1);
                var individualReport = report.IndividualReports[0];
                individualReport.TotalResolved.Should().Be(5);
                individualReport.TotalResolvedBugs.Should().Be(3);
                individualReport.TotalResolvedTasks.Should().Be(2);
            });
        }

        [Test]
        public async Task ShouldCountEstimatedTimeAndDuration()
        {
            var emmy = Builder<TeamMemberViewModel>.CreateNew()
                .Build();
            var profile = Builder<ProfileViewModel>.CreateNew()
                .With(p => p.Members, new[] { emmy.Id })
                .Build();

            SetupGetProfile(profile);
            SetupGetTeamMember(new[] { emmy });
            var workitemsForEmmy = new WorkItemTestDataContainer(new[]
            {
                CreateBug().WithNormalLifecycle(emmy, 3).WithETA(2, 5, 1),
                CreateTask().WithNormalLifecycle(emmy, 1).WithETA(0, 2, 1),
                CreateBug().WithNormalLifecycle(emmy, 2).WithETA(0, 7, 0),
                CreateTask().WithNormalLifecycle(emmy, 5).WithETA(3, 0, 1),
                CreateBug().WithNormalLifecycle(emmy, 4).WithETA(6, 0, 2),
                CreateBug().WithNormalLifecycle(emmy, 1),
                CreateBug().WithNormalLifecycle(emmy, 9)
            });
            SetupGetWorkitems(emmy.Id, workitemsForEmmy.WorkItems);
            SetupClassify(workitemsForEmmy);

            await InvokeAndVerify<AggregatedWorkitemsETAReport>(Command, (report, reportId) =>
            {
                report.IndividualReports.Should().HaveCount(1);
                var individualReport = report.IndividualReports[0];
                individualReport.CompletedWithEstimates.Should().Be(15);
                individualReport.CompletedWithoutEstimates.Should().Be(10);
                individualReport.EstimatedToComplete.Should().Be(19);
                individualReport.OriginalEstimated.Should().Be(11);
            });
        }

        [Test]
        public async Task ShouldPopulateDetails()
        {
            var jim = Builder<TeamMemberViewModel>.CreateNew()
                .Build();
            var profile = Builder<ProfileViewModel>.CreateNew()
                .With(p => p.Members, new[] { jim.Id })
                .Build();

            SetupGetProfile(profile);
            SetupGetTeamMember(new[] { jim });
            SetupJim(jim);

            await InvokeAndVerify<AggregatedWorkitemsETAReport>(Command, (report, reportId) =>
            {
                report.IndividualReports.Should().HaveCount(1);
                var individualReport = report.IndividualReports[0];
                var bug = individualReport.Details[0];
                var task = individualReport.Details[1];

                bug.WorkItemType.Should().Be(Constants.WorkItemTypeBug);
                bug.WorkItemTitle.Should().NotBeEmpty();
                bug.WorkItemId.Should().NotBe(default(int));
                bug.OriginalEstimate.Should().Be(0);
                bug.EstimatedToComplete.Should().Be(5);
                bug.TimeSpent.Should().Be(3);

                task.WorkItemType.Should().Be(Constants.WorkItemTypeTask);
                task.WorkItemTitle.Should().NotBeEmpty();
                task.WorkItemId.Should().NotBe(default(int));
                task.OriginalEstimate.Should().Be(0);
                task.EstimatedToComplete.Should().Be(2);
                task.TimeSpent.Should().Be(5);
            });
        }

        [Test]
        public async Task ShouldReturnEmptyIndividualReportIfNoResolutions()
        {
            var victoria = Builder<TeamMemberViewModel>.CreateNew()
                .Build();
            var profile = Builder<ProfileViewModel>.CreateNew()
                .With(p => p.Members, new[] { victoria.Id })
                .Build();

            SetupGetProfile(profile);
            SetupGetTeamMember(new[] { victoria });
            var expectedValuesForVictoria = SetupVictoria(victoria);

            await InvokeAndVerify<AggregatedWorkitemsETAReport>(Command, (report, reportId) =>
            {
                report.IndividualReports.Should().HaveCount(1);
                VerifyReportFor(victoria, report, expectedValuesForVictoria);
            });
        }

        protected override GenerateAggregatedWorkitemsETAReportHandler InitializeHandler()
        {
            var realDataSource = new VstsDataSource(RepositoryMock.Object, Mapper);

            DataSourceMock.Setup(d => d.GetActiveDuration(It.IsAny<WorkItemViewModel>()))
                .Returns<WorkItemViewModel>(w => realDataSource.GetActiveDuration(w));
            DataSourceMock.Setup(d => d.GetETAValues(It.IsAny<WorkItemViewModel>()))
                .Returns<WorkItemViewModel>(w => realDataSource.GetETAValues(w));
            _classificationContextMock = new Mock<IWorkItemClassificationContext>(MockBehavior.Strict);
            return new GenerateAggregatedWorkitemsETAReportHandler(
                DataSourceProviderMock.Object,
                _classificationContextMock.Object,
                RepositoryMock.Object,
                GetLoggerMock<GenerateAggregatedWorkitemsETAReportHandler>());
        }

        private void SetupClassify(WorkItemTestDataContainer dataContainer)
        {
            _classificationContextMock.Setup(c => c.Classify(It.Is<WorkItemViewModel>(w => dataContainer.WorkItems.Any(wi => wi.WorkItemId == w.WorkItemId)), It.IsAny<ClassificationScope>()))
                .Returns<WorkItemViewModel, ClassificationScope>((wi, _) => dataContainer.ResolutionsById(wi.WorkItemId));
        }

        private void VerifyReportFor(TeamMemberViewModel member, AggregatedWorkitemsETAReport report, ExpectedValuesContainer expectedValues)
        {
            var memberReport = report.IndividualReports.Single(r => r.MemberEmail == member.Email);
            memberReport.TotalResolved.Should().Be(expectedValues.GetValue<int>(nameof(memberReport.TotalResolved)));
            memberReport.CompletedWithEstimates.Should().Be(expectedValues.GetValue<int>(nameof(memberReport.CompletedWithEstimates)));
            memberReport.CompletedWithoutEstimates.Should().Be(expectedValues.GetValue<int>(nameof(memberReport.CompletedWithoutEstimates)));
            memberReport.EstimatedToComplete.Should().Be(expectedValues.GetValue<int>(nameof(memberReport.EstimatedToComplete)));
            memberReport.OriginalEstimated.Should().Be(expectedValues.GetValue<int>(nameof(memberReport.OriginalEstimated)));
            memberReport.WithOriginalEstimate.Should().Be(expectedValues.GetValue<int>(nameof(memberReport.WithOriginalEstimate)));
            memberReport.WithoutETA.Should().Be(expectedValues.GetValue<int>(nameof(memberReport.WithoutETA)));
            memberReport.TotalResolvedBugs.Should().Be(expectedValues.GetValue<int>(nameof(memberReport.TotalResolvedBugs)));
            memberReport.TotalResolvedTasks.Should().Be(expectedValues.GetValue<int>(nameof(memberReport.TotalResolvedTasks)));
            memberReport.MemberEmail.Should().Be(expectedValues.GetValue<string>(nameof(memberReport.MemberEmail)));
            memberReport.MemberName.Should().Be(expectedValues.GetValue<string>(nameof(memberReport.MemberName)));
            memberReport.Details.Should().HaveCount(expectedValues.GetValue<int>(nameof(memberReport.Details)));
        }

        private ExpectedValuesContainer SetupBob(TeamMemberViewModel bob)
        {
            var workitemsForBob = new WorkItemTestDataContainer(new[]
            {
                CreateBug().WithNormalLifecycle(bob, 7).WithETA(0, 4, 0),
                CreateTask().WithNormalLifecycle(bob, 16).WithETA(0, 1, 0),
                CreateTask().WithNormalLifecycle(bob, 1).WithETA(0, 1, 1)
            });
            SetupGetWorkitems(bob.Id, workitemsForBob.WorkItems);
            SetupClassify(workitemsForBob);

            var expectedValuesForBob = new ExpectedValuesContainer(new Dictionary<string, object>
            {
                { nameof(AggregatedWorkitemsETAReport.IndividualETAReport.TotalResolved), 3 },
                { nameof(AggregatedWorkitemsETAReport.IndividualETAReport.CompletedWithEstimates), workitemsForBob.ExpectedDuration },
                { nameof(AggregatedWorkitemsETAReport.IndividualETAReport.CompletedWithoutEstimates), 0 },
                { nameof(AggregatedWorkitemsETAReport.IndividualETAReport.EstimatedToComplete), workitemsForBob.ExpectedEstimatedToComplete },
                { nameof(AggregatedWorkitemsETAReport.IndividualETAReport.OriginalEstimated), workitemsForBob.ExpectedOriginalEstimate },
                { nameof(AggregatedWorkitemsETAReport.IndividualETAReport.WithOriginalEstimate), 0 },
                { nameof(AggregatedWorkitemsETAReport.IndividualETAReport.WithoutETA), 0 },
                { nameof(AggregatedWorkitemsETAReport.IndividualETAReport.TotalResolvedBugs), 1 },
                { nameof(AggregatedWorkitemsETAReport.IndividualETAReport.TotalResolvedTasks), 2 },
                { nameof(AggregatedWorkitemsETAReport.IndividualETAReport.MemberEmail), bob.Email },
                { nameof(AggregatedWorkitemsETAReport.IndividualETAReport.MemberName), bob.DisplayName },
                { nameof(AggregatedWorkitemsETAReport.IndividualETAReport.Details), 3 }
            });
            return expectedValuesForBob;
        }

        private ExpectedValuesContainer SetupJim(TeamMemberViewModel jim)
        {
            var workitemsForJim = new WorkItemTestDataContainer(new[]
            {
                CreateBug().WithNormalLifecycle(jim, 3).WithETA(0, 5, 0),
                CreateTask().WithNormalLifecycle(jim, 5).WithETA(0, 2, 0)
            });
            SetupGetWorkitems(jim.Id, workitemsForJim.WorkItems);
            SetupClassify(workitemsForJim);

            var expectedValuesForJim = new ExpectedValuesContainer(new Dictionary<string, object>
            {
                { nameof(AggregatedWorkitemsETAReport.IndividualETAReport.TotalResolved), 2 },
                { nameof(AggregatedWorkitemsETAReport.IndividualETAReport.CompletedWithEstimates), workitemsForJim.ExpectedDuration },
                { nameof(AggregatedWorkitemsETAReport.IndividualETAReport.CompletedWithoutEstimates), 0 },
                { nameof(AggregatedWorkitemsETAReport.IndividualETAReport.EstimatedToComplete), workitemsForJim.ExpectedEstimatedToComplete },
                { nameof(AggregatedWorkitemsETAReport.IndividualETAReport.OriginalEstimated), workitemsForJim.ExpectedOriginalEstimate },
                { nameof(AggregatedWorkitemsETAReport.IndividualETAReport.WithOriginalEstimate), 0 },
                { nameof(AggregatedWorkitemsETAReport.IndividualETAReport.WithoutETA), 0 },
                { nameof(AggregatedWorkitemsETAReport.IndividualETAReport.TotalResolvedBugs), 1 },
                { nameof(AggregatedWorkitemsETAReport.IndividualETAReport.TotalResolvedTasks), 1 },
                { nameof(AggregatedWorkitemsETAReport.IndividualETAReport.MemberEmail), jim.Email },
                { nameof(AggregatedWorkitemsETAReport.IndividualETAReport.MemberName), jim.DisplayName },
                { nameof(AggregatedWorkitemsETAReport.IndividualETAReport.Details), 2 }
            });
            return expectedValuesForJim;
        }

        private ExpectedValuesContainer SetupVictoria(TeamMemberViewModel victoria)
        {
            var workitemsForVictoria = new WorkItemTestDataContainer(new[]
            {
                CreateBug(),
                CreateTask()
            });
            SetupGetWorkitems(victoria.Id, workitemsForVictoria.WorkItems);
            SetupClassify(workitemsForVictoria);

            var expectedValuesForVictoria = new ExpectedValuesContainer(new Dictionary<string, object>
            {
                { nameof(AggregatedWorkitemsETAReport.IndividualETAReport.TotalResolved), 0 },
                { nameof(AggregatedWorkitemsETAReport.IndividualETAReport.CompletedWithEstimates), workitemsForVictoria.ExpectedDuration },
                { nameof(AggregatedWorkitemsETAReport.IndividualETAReport.CompletedWithoutEstimates), 0 },
                { nameof(AggregatedWorkitemsETAReport.IndividualETAReport.EstimatedToComplete), workitemsForVictoria.ExpectedEstimatedToComplete },
                { nameof(AggregatedWorkitemsETAReport.IndividualETAReport.OriginalEstimated), workitemsForVictoria.ExpectedOriginalEstimate },
                { nameof(AggregatedWorkitemsETAReport.IndividualETAReport.WithOriginalEstimate), 0 },
                { nameof(AggregatedWorkitemsETAReport.IndividualETAReport.WithoutETA), 0 },
                { nameof(AggregatedWorkitemsETAReport.IndividualETAReport.TotalResolvedBugs), 0 },
                { nameof(AggregatedWorkitemsETAReport.IndividualETAReport.TotalResolvedTasks), 0 },
                { nameof(AggregatedWorkitemsETAReport.IndividualETAReport.MemberEmail), victoria.Email },
                { nameof(AggregatedWorkitemsETAReport.IndividualETAReport.MemberName), victoria.DisplayName },
                { nameof(AggregatedWorkitemsETAReport.IndividualETAReport.Details), 0 }
            });
            return expectedValuesForVictoria;
        }
    }
}
