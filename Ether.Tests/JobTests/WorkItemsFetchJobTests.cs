using Ether.Core.Configuration;
using Ether.Core.Interfaces;
using Ether.Core.Models.DTO;
using Ether.Core.Models.VSTS;
using Ether.Jobs;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using static Ether.Jobs.WorkItemsQueryResponse;

namespace Ether.Tests.JobTests
{
    [TestFixture]
    public class WorkItemsFetchJobTests
    {
        Mock<IRepository> _repositoryMock;
        Mock<IVSTSClient> _vstsClientMock;
        Mock<IOptions<VSTSConfiguration>> _configurationMock;
        Mock<ILogger<WorkItemsFetchJob>> _loggerMock;
        WorkItemsFetchJob _job;

        [SetUp]
        public void SetUp()
        {
            _repositoryMock = new Mock<IRepository>(MockBehavior.Strict);
            _vstsClientMock = new Mock<IVSTSClient>(MockBehavior.Strict);
            _configurationMock = new Mock<IOptions<VSTSConfiguration>>(MockBehavior.Strict);
            _loggerMock = new Mock<ILogger<WorkItemsFetchJob>>();

            _job = new WorkItemsFetchJob(_repositoryMock.Object, _vstsClientMock.Object, _configurationMock.Object, _loggerMock.Object);

            _repositoryMock.Setup(r => r.GetFieldValue<Settings, bool>(_ => true, s => s.WorkItemsSettings.DisableWorkitemsJob)).Returns(false);
        }

        [Test]
        public void ShouldExitIfJobIsDisabled()
        {
            _configurationMock.SetupGet(c => c.Value).Returns(new VSTSConfiguration { AccessToken = "foo", InstanceName = "bar" });
            _repositoryMock.Setup(r => r.GetFieldValue<Settings, bool>(_ => true, s => s.WorkItemsSettings.DisableWorkitemsJob)).Returns(true);

            _job.Execute();

            _loggerMock.Verify(l => l.Log(LogLevel.Warning, 0, It.IsAny<FormattedLogValues>(), null, It.IsAny<Func<object, Exception, string>>()), Times.Once());
            _repositoryMock.Verify(r => r.GetAll<TeamMember>(), Times.Never());
            _repositoryMock.Verify(r => r.Get(It.IsAny<Expression<Func<VSTSProject, bool>>>()), Times.Never());
        }

        [Test]
        public void ShouldExitIfConfigurationIsInvalid()
        {
            _configurationMock.SetupGet(c => c.Value).Returns(new VSTSConfiguration());

            _job.Execute();

            _loggerMock.Verify(l => l.Log(LogLevel.Error, 0, It.IsAny<FormattedLogValues>(), null, It.IsAny<Func<object, Exception, string>>()), Times.Once());
            _repositoryMock.Verify(r => r.GetAll<TeamMember>(), Times.Never());
            _repositoryMock.Verify(r => r.Get(It.IsAny<Expression<Func<VSTSProject, bool>>>()), Times.Never());
        }

        [Test]
        public void ShouldCatchExceptionsThrownByRepository()
        {
            _configurationMock.SetupGet(c => c.Value).Returns(new VSTSConfiguration { AccessToken = "foo", InstanceName = "bar" });

            _repositoryMock.Setup(r => r.GetAll<TeamMember>()).Throws<Exception>();
            _repositoryMock.Setup(r => r.Get(It.IsAny<Expression<Func<VSTSProject, bool>>>())).Throws<Exception>();

            _job.Invoking(j => j.Execute())
                .Should()
                .NotThrow();

            _loggerMock.Verify(l => l.Log(LogLevel.Error, 0, It.IsAny<FormattedLogValues>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Once());
        }

        [Test]
        public void ShouldNotThrowIfConfigurationIsNull()
        {

            _configurationMock.SetupGet(c => c.Value).Returns((VSTSConfiguration)null);
            _job.Invoking(j => j.Execute()).Should().NotThrow();
        }

        [Test]
        public void ShouldCatchExceptionWhileFetchingDataForUserAndContinue()
        {
            _configurationMock.SetupGet(c => c.Value).Returns(new VSTSConfiguration { AccessToken = "foo", InstanceName = "bar" });
            _repositoryMock.Setup(r => r.GetAll<TeamMember>()).Returns(new[] { new TeamMember { Email = "foo@bar.com" }, new TeamMember { Email = "baz@fiz.com" } });
            _repositoryMock.Setup(r => r.Get(It.IsAny<Expression<Func<VSTSProject, bool>>>())).Returns(new[] { new VSTSProject { Name = "Foo" } });
            _vstsClientMock.Setup(c => c.ExecutePost<WorkItemsQueryResponse>(It.IsAny<string>(), It.IsAny<ItemsQuery>())).Throws<Exception>();

            _job.Execute();

            _configurationMock.VerifyAll();
            _repositoryMock.Verify(r => r.GetAll<TeamMember>(), Times.Once());
            _repositoryMock.Verify(r => r.Get(It.IsAny<Expression<Func<VSTSProject, bool>>>()), Times.Once());
            _vstsClientMock.Verify(c => c.ExecutePost<WorkItemsQueryResponse>(It.IsAny<string>(), It.IsAny<ItemsQuery>()), Times.Exactly(2));
            _loggerMock.Verify(l => l.Log(LogLevel.Error, 0, It.IsAny<FormattedLogValues>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Exactly(2));
        }

        [Test]
        public void ShouldFetchRelatedWorkItemsForAllMembers()
        {
            const string lastFetch = "07/10/2017";
            var lastFetchDate = DateTime.Parse(lastFetch);
            var user1 = new TeamMember { Email = "foo@bar.com", LastFetchDate = lastFetchDate };
            var user2 = new TeamMember { Email = "baz@fiz.com", LastFetchDate = lastFetchDate };

            CreateSetupForRelatedWorkitems(user1, user2, lastFetch);

            _job.Execute();

            user1.LastFetchDate.Should().BeCloseTo(DateTime.UtcNow);
            user2.LastFetchDate.Should().BeCloseTo(DateTime.UtcNow);

            user1.RelatedWorkItemIds.Should().BeEquivalentTo(new[] { 1, 2, 3 });
            user2.RelatedWorkItemIds.Should().BeEquivalentTo(new[] { 4, 5 });

            _repositoryMock.Verify();
            _repositoryMock.Verify(r => r.CreateOrUpdateAsync(It.IsAny<TeamMember>()), Times.Exactly(2));
            _vstsClientMock.Verify(c => c.ExecutePost<WorkItemsQueryResponse>(It.IsAny<string>(), It.IsAny<ItemsQuery>()), Times.Exactly(2));
        }

        [Test]
        public void ShouldUpdateRelatedWorkItemsForAllMembers()
        {
            const string lastFetch = "07/10/2017";
            var lastFetchDate = DateTime.Parse(lastFetch);

            var user1 = new TeamMember { Email = "foo@bar.com", LastFetchDate = lastFetchDate, RelatedWorkItemIds = new[] { 7, 1 } };
            var user2 = new TeamMember { Email = "baz@fiz.com", LastFetchDate = lastFetchDate, RelatedWorkItemIds = new[] { 5, 6 } };

            CreateSetupForRelatedWorkitems(user1, user2, lastFetch);

            _job.Execute();

            user1.RelatedWorkItemIds.Should().BeEquivalentTo(new[] { 1, 2, 3, 7 });
            user2.RelatedWorkItemIds.Should().BeEquivalentTo(new[] { 4, 5, 6 });

            _repositoryMock.Verify();
            _repositoryMock.Verify(r => r.CreateOrUpdateAsync(It.IsAny<TeamMember>()), Times.Exactly(2));
            _vstsClientMock.Verify(c => c.ExecutePost<WorkItemsQueryResponse>(It.IsAny<string>(), It.IsAny<ItemsQuery>()), Times.Exactly(2));
        }

        [Test]
        public void ShouldQueryOnlyChangedWorkItems()
        {
            const string lastFetch = "07/10/2017";
            var lastFetchDate = DateTime.Parse(lastFetch);
            var user = new TeamMember { Email = "baz@fiz.com", LastFetchDate = lastFetchDate, RelatedWorkItemIds = new[] { 1, 2, 3 } };
            var recentlyChangedWorkItems = new[] { new WorkItemLink { Id = 4 }, new WorkItemLink { Id = 5 } };

            _configurationMock.SetupGet(c => c.Value).Returns(new VSTSConfiguration { AccessToken = "foo", InstanceName = "bar" });
            _repositoryMock.Setup(r => r.GetAll<TeamMember>()).Returns(new[] { user });
            _repositoryMock.Setup(r => r.Get(It.IsAny<Expression<Func<VSTSProject, bool>>>()))
                .Returns(new[] { new VSTSProject { Name = "Foo" } })
                .Verifiable();
            _repositoryMock.Setup(r => r.CreateOrUpdateAsync(It.IsAny<TeamMember>()))
                .Returns(Task.FromResult(true));
            _vstsClientMock.Setup(c => c.ExecutePost<WorkItemsQueryResponse>(It.IsAny<string>(), It.IsAny<ItemsQuery>()))
                .Returns(Task.FromResult(new WorkItemsQueryResponse { WorkItems = recentlyChangedWorkItems }));
            _vstsClientMock.Setup(c => c.ExecuteGet<ValueResponse<VSTSWorkItem>>(It.IsAny<string>()))
                .Returns(Task.FromResult(new ValueResponse<VSTSWorkItem> { Value = new VSTSWorkItem[0] }));

            _job.Execute();

            user.RelatedWorkItemIds.Should().HaveCount(5);
            _vstsClientMock.Verify(c => c.ExecuteGet<ValueResponse<VSTSWorkItem>>(It.Is<string>(s => CheckForCorrectIds(s, "ids=4,5"))), Times.Once());
        }

        [Test]
        public void ShouldTakeDefaultYearIfNotSpecified()
        {
            var lastFetchDate = DateTime.UtcNow.AddYears(-10);
            var user1 = new TeamMember { Email = "foo@bar.com", LastFetchDate = DateTime.MinValue, RelatedWorkItemIds = new[] { 7, 1 } };
            var user2 = new TeamMember { Email = "baz@fiz.com", LastFetchDate = DateTime.MinValue, RelatedWorkItemIds = new[] { 5, 6 } };

            var expectedFetchDate = lastFetchDate.ToString("MM/dd/yyyy");
            CreateSetupForRelatedWorkitems(user1, user2, expectedFetchDate);

            _job.Execute();

            _vstsClientMock.Verify(c => c.ExecutePost<WorkItemsQueryResponse>(It.IsAny<string>(), It.Is<ItemsQuery>(q => q.Query.Contains(expectedFetchDate))), Times.Exactly(2));
        }

        [Test]
        public void ShouldQueryRelatedWorkItemsForEveryProject()
        {
            var user1 = new TeamMember { Email = "foo@bar.com" };
            var user2 = new TeamMember { Email = "baz@fiz.com" };

            var expectedFetchDate = DateTime.UtcNow.AddYears(-10).ToString("MM/dd/yyyy");
            CreateSetupForRelatedWorkitems(user1, user2, expectedFetchDate, numberOfProjects: 2);

            _job.Execute();

            _vstsClientMock.Verify(c => c.ExecutePost<WorkItemsQueryResponse>(It.Is<string>(url => url.Contains("Foo_1")), It.IsAny<ItemsQuery>()), Times.Exactly(2));
            _vstsClientMock.Verify(c => c.ExecutePost<WorkItemsQueryResponse>(It.Is<string>(url => url.Contains("Foo_2")), It.IsAny<ItemsQuery>()), Times.Exactly(2));
        }

        [TestCase(0, 0)]
        [TestCase(1, 1)]
        [TestCase(10, 1)]
        [TestCase(100, 1)]
        [TestCase(200, 1)]
        [TestCase(235, 2)]
        [TestCase(400, 2)]
        [TestCase(435, 3)]
        [TestCase(543, 3)]
        public void ShouldFetchWorkitemsInChunksWithNoMoreThan200Items(int itemsCount, int expectedIterations)
        {
            const string expectedFileds = "System.Id,System.WorkItemType,System.Title,System.AreaPath,System.ChangedDate,System.Tags,System.State,System.Reason," +
                "System.CreatedDate,Microsoft.VSTS.Common.ResolvedDate,Microsoft.VSTS.Common.ClosedDate,Microsoft.VSTS.Common.StateChangeDate," +
                "Microsoft.VSTS.Scheduling.OriginalEstimate,Microsoft.VSTS.Scheduling.CompletedWork,Microsoft.VSTS.Scheduling.RemainingWork";
            var expectedWorkItems = Enumerable.Range(1, itemsCount).Select(i => i);
            var workItemsLinks = expectedWorkItems.Select(i => new WorkItemLink { Id = i }).ToArray();
            _configurationMock.SetupGet(c => c.Value).Returns(new VSTSConfiguration { AccessToken = "foo", InstanceName = "bar" });
            _repositoryMock.Setup(r => r.GetAll<TeamMember>()).Returns(new[] { new TeamMember { Email = "foo@bar.com" } });
            _repositoryMock.Setup(r => r.Get(It.IsAny<Expression<Func<VSTSProject, bool>>>())).Returns(new[] { new VSTSProject { Name = $"Foo" } })
                .Verifiable();
            _repositoryMock.Setup(r => r.CreateOrUpdateAsync(It.IsAny<TeamMember>()))
                .Returns(Task.FromResult(true));
            _vstsClientMock.Setup(c => c.ExecutePost<WorkItemsQueryResponse>(It.IsAny<string>(), It.IsAny<ItemsQuery>()))
                .Returns<string, ItemsQuery>((u, q) => Task.FromResult(new WorkItemsQueryResponse { WorkItems = workItemsLinks }));
            _vstsClientMock.Setup(c => c.ExecuteGet<ValueResponse<VSTSWorkItem>>(It.IsAny<string>()))
                .Returns(Task.FromResult(new ValueResponse<VSTSWorkItem> { Value = new VSTSWorkItem[0] }));

            _job.Execute();


            _vstsClientMock.Verify(c => c.ExecuteGet<ValueResponse<VSTSWorkItem>>(It.Is<string>(s=>s.Contains(expectedFileds))), Times.Exactly(expectedIterations));
        }

        [Test]
        public void ShouldFetchWorkitemsUpdates()
        {
            const int itemsCount = 5;
            var expectedWorkItems = Enumerable.Range(1, itemsCount).Select(i => i);
            var workItemsLinks = expectedWorkItems.Select(i => new WorkItemLink { Id = i }).ToArray();
            var workItems = expectedWorkItems.Select(i => new VSTSWorkItem { WorkItemId = i }).ToArray();
            _configurationMock.SetupGet(c => c.Value).Returns(new VSTSConfiguration { AccessToken = "foo", InstanceName = "bar" });
            _repositoryMock.Setup(r => r.GetAll<TeamMember>()).Returns(new[] { new TeamMember { Email = "foo@bar.com" } });
            _repositoryMock.Setup(r => r.Get(It.IsAny<Expression<Func<VSTSProject, bool>>>())).Returns(new[] { new VSTSProject { Name = $"Foo" } })
                .Verifiable();
            _repositoryMock.Setup(r => r.CreateOrUpdateAsync(It.IsAny<TeamMember>()))
                .Returns(Task.FromResult(true));
            _repositoryMock.Setup(r => r.CreateOrUpdateAsync(It.IsAny<VSTSWorkItem>(), It.IsAny<Expression<Func<VSTSWorkItem, bool>>>()))
                .Returns(Task.FromResult(true));
            _vstsClientMock.Setup(c => c.ExecutePost<WorkItemsQueryResponse>(It.IsAny<string>(), It.IsAny<ItemsQuery>()))
                .Returns<string, ItemsQuery>((u, q) => Task.FromResult(new WorkItemsQueryResponse { WorkItems = workItemsLinks }));
            _vstsClientMock.Setup(c => c.ExecuteGet<ValueResponse<VSTSWorkItem>>(It.IsAny<string>()))
                .Returns(Task.FromResult(new ValueResponse<VSTSWorkItem> { Value = workItems }));
            _vstsClientMock.Setup(c => c.ExecuteGet<ValueResponse<WorkItemUpdate>>(It.IsAny<string>()))
                .Returns(Task.FromResult(new ValueResponse<WorkItemUpdate> { Value = new[] { new WorkItemUpdate(), new WorkItemUpdate() } }));

            _job.Execute();

            _repositoryMock.Verify(r => r.CreateOrUpdateAsync(It.Is<VSTSWorkItem>(wi => wi.Updates != null && wi.Updates.Count() == 2),
                    It.Is<Expression<Func<VSTSWorkItem, bool>>>(e => CheckExpressionForWorkItemSave(e))), Times.Exactly(itemsCount));
        }

        [Test]
        public void ShouldRunJobOnlyForSpecifiedUser()
        {
            const string lastFetch = "07/10/2017";
            var lastFetchDate = DateTime.Parse(lastFetch);
            var user1 = new TeamMember { Email = "foo@bar.com", LastFetchDate = lastFetchDate };
            var user2 = new TeamMember { Email = "baz@fiz.com", LastFetchDate = lastFetchDate };
            var expectedUser = new TeamMember { Email = "specific@bar.com", LastFetchDate = lastFetchDate };
            CreateSetupForRelatedWorkitems(user1, user2, lastFetch);

            _job.SpecificUser = expectedUser;
            _job.Execute();

            user1.LastFetchDate.Should().BeCloseTo(lastFetchDate);
            user2.LastFetchDate.Should().BeCloseTo(lastFetchDate);
            expectedUser.LastFetchDate.Should().BeCloseTo(DateTime.UtcNow);

            _repositoryMock.Verify();
            _repositoryMock.Verify(r => r.CreateOrUpdateAsync(It.Is<TeamMember>(u=>u.Email == expectedUser.Email)), Times.Once());
            _vstsClientMock.Verify(c => c.ExecutePost<WorkItemsQueryResponse>(It.IsAny<string>(), It.IsAny<ItemsQuery>()), Times.Exactly(1));
        }

        private bool CheckExpressionForWorkItemSave(Expression<Func<VSTSWorkItem, bool>> e)
        {
            var body = e.Body as BinaryExpression;
            if(body == null)
                return false;

            var memberLeft = body.Left as MemberExpression;
            var memberRight = body.Right as MemberExpression;
            if (memberLeft == null || memberRight == null )
                return false;

            return memberLeft.Member.Name == nameof(VSTSWorkItem.WorkItemId) && memberRight.Member.Name == nameof(VSTSWorkItem.WorkItemId);
        }

        private void CreateSetupForRelatedWorkitems(TeamMember member1, TeamMember member2, string lastFetch, int numberOfProjects = 1)
        {
            var wiQuery = $@"SELECT [System.Id] FROM WorkItems 
                            WHERE [System.WorkItemType] IN ('Bug', 'Task') AND [System.AssignedTo] Ever '{{0}}' AND System.ChangedDate >= '{lastFetch}'";
            var expectedUser1Ids = new[] { new WorkItemLink { Id = 1 }, new WorkItemLink { Id = 2 }, new WorkItemLink { Id = 3 } };
            var expectedUser2Ids = new[] { new WorkItemLink { Id = 4 }, new WorkItemLink { Id = 5 } };

            _configurationMock.SetupGet(c => c.Value).Returns(new VSTSConfiguration { AccessToken = "foo", InstanceName = "bar" });
            _repositoryMock.Setup(r => r.GetAll<TeamMember>()).Returns(new[] { member1, member2 });

            var projects = Enumerable.Range(1, numberOfProjects).Select(i => new VSTSProject { Name = $"Foo_{i}" });
            _repositoryMock.Setup(r => r.Get(It.IsAny<Expression<Func<VSTSProject, bool>>>())).Returns(projects)
                .Verifiable();
            _repositoryMock.Setup(r => r.CreateOrUpdateAsync(It.IsAny<TeamMember>()))
                .Returns(Task.FromResult(true));
            _vstsClientMock.Setup(c => c.ExecutePost<WorkItemsQueryResponse>(It.IsAny<string>(), It.IsAny<ItemsQuery>()))
                .Returns<string, ItemsQuery>((u, q) => Task.FromResult(new WorkItemsQueryResponse
                {
                    WorkItems = q.Query == string.Format(wiQuery, member1.Email) ? expectedUser1Ids : expectedUser2Ids
                }));
            _vstsClientMock.Setup(c => c.ExecuteGet<ValueResponse<VSTSWorkItem>>(It.IsAny<string>()))
                .Returns(Task.FromResult(new ValueResponse<VSTSWorkItem> { Value = new VSTSWorkItem[0] }));
        }

        private bool CheckForCorrectIds(string url, string expectedIds)
        {
            var uri = new Uri(url);
            var idsSection = uri.Query
                .TrimStart('?')
                .Split('&')
                .SingleOrDefault(q => q.StartsWith("ids"));

            return string.Equals(expectedIds, idsSection, StringComparison.OrdinalIgnoreCase);
        }
    }
}
