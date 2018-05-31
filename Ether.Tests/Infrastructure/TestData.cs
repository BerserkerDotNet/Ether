using Ether.Core.Configuration;
using Ether.Core.Constants;
using Ether.Core.Interfaces;
using Ether.Core.Models;
using Ether.Core.Models.DTO;
using Ether.Core.Models.DTO.Reports;
using Ether.Core.Models.VSTS;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ether.Tests.Infrastructure
{
    public class TestData
    {

        public const string MembersCountKey = "MembersCount";
        public const string RepositoryCountKey = "RepositoryCount";
        public const string ProjectsCountKey = "ProjectsCount";
        public const string RelatedWorkItemsCountKey = "RelatedWorkItemsCount";
        public const string DefaultInstanceName = "Foo";
        public const string DefaultToken = "SecureToken";
        private static readonly Random _random = new Random();

        public IEnumerable<TeamMember> TeamMembers { get; private set; }

        public IEnumerable<Guid> TeamMemberIds => TeamMembers.Select(m => m.Id);

        public IEnumerable<VSTSRepository> Repositories { get; private set; }

        public IEnumerable<Guid> RepositoryIds => Repositories.Select(m => m.Id);

        public IEnumerable<VSTSProject> Projects { get; private set; }

        public IEnumerable<Guid> ProjectIds => Projects.Select(m => m.Id);

        public Profile Profile { get; private set; }

        public Mock<IRepository> RepositoryMock { get; private set; }

        public Mock<IOptions<VSTSConfiguration>> ConfigurationMock { get; private set; }

        public TestData WithBasicData(int membersCount, int repositoryCount, int projectsCount)
        {
            TeamMembers = CreateTeamMembers(membersCount);
            Projects = CreateProjects(projectsCount);
            Repositories = CreateRepositories(repositoryCount);
            Profile = CreateProfile();

            return this;
        }

        public TestData WithRepositoryMocks()
        {
            RepositoryMock = new Mock<IRepository>(MockBehavior.Strict);
            RepositoryMock.Setup(r => r.CreateAsync(It.IsAny<ReportResult>())).Returns(Task.FromResult(true));
            RepositoryMock.Setup(r => r.GetSingleAsync<Profile>(It.IsAny<Guid>())).Returns(Task.FromResult(Profile));
            RepositoryMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<VSTSRepository, bool>>>())).Returns(Task.FromResult(Repositories));
            RepositoryMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<VSTSProject, bool>>>())).Returns(Task.FromResult(Projects));
            RepositoryMock.Setup(c => c.GetAsync(It.IsAny<Expression<Func<TeamMember, bool>>>())).Returns(Task.FromResult(TeamMembers));
            return this;
        }

        public TestData WithConfiguration(string instanceName = DefaultInstanceName, string token = DefaultToken)
        {
            ConfigurationMock = new Mock<IOptions<VSTSConfiguration>>();
            ConfigurationMock.SetupGet(c => c.Value).Returns(new VSTSConfiguration { AccessToken = token, InstanceName = instanceName });
            return this;
        }

        public TestData WithRelatedWorkItems(int relatedWorkItemsCount)
        {
            if (TeamMembers == null || !TeamMembers.Any() || relatedWorkItemsCount == 0)
                return this;

            foreach (var member in TeamMembers)
            {
                member.RelatedWorkItemIds = Enumerable.Range(0, relatedWorkItemsCount)
                    .Select(_ => _random.Next(100_000_000))
                    .ToList();
            }

            return this;
        }

        public ReportQuery GetDefaultQuery() => new ReportQuery { StartDate = DateTime.MinValue, EndDate = DateTime.MaxValue, ProfileId = Profile.Id };

        public VSTSWorkItem GetWorkItemFor(TeamMember user, string type = WorkItemTypes.Bug)
        {
            var resolvedWorkItem = new VSTSWorkItem { Id = Guid.NewGuid(), WorkItemId = user.RelatedWorkItemIds.Random(), Fields = new Dictionary<string, string>() };
            resolvedWorkItem.Fields.Add(VSTSFieldNames.WorkItemType, type);
            resolvedWorkItem.Fields.Add(VSTSFieldNames.WorkItemCreatedDate, DateTime.UtcNow.AddDays(-_random.Next(1, 15)).ToString());

            return resolvedWorkItem;
        }

        public IEnumerable<WorkItemUpdate> GetBugFullCycle(TeamMember resolvedBy, TeamMember closedBy, DateTime? resolvedOn = null)
        {
            return UpdateBuilder.Create()
                  .Activated().Then()
                  .Resolved(resolvedBy).On(resolvedOn ?? DateTime.UtcNow)
                  .Then()
                  .Closed(closedBy, reason: Constants.ClosedBugReason).On(DateTime.UtcNow)
                  .Build();
        }

        public IEnumerable<TeamMember> CreateTeamMembers(int count)
        {
            return Enumerable.Range(0, count)
                .Select(i =>
                {
                    var id = Guid.NewGuid();
                    return new TeamMember { Id = id, Email = $"member{id.ToString("N")}@foo.com", DisplayName = $"Member {id.ToString("N")}" };
                }).ToList();
        }

        private IEnumerable<VSTSRepository> CreateRepositories(int count)
        {
            return Enumerable.Range(0, count)
                .Select(i =>
                {
                    var projectToTake = _random.Next(1, ProjectIds.Count() + 1);
                    var projectId = ProjectIds.Any() ? ProjectIds.Take(projectToTake).Last() : Guid.Empty;

                    return new VSTSRepository
                    {
                        Id = Guid.NewGuid(),
                        Name = $"Repository {i}",
                        Project = projectId
                    };
                }).ToList();
        }

        private IEnumerable<VSTSProject> CreateProjects(int count)
        {
            return Enumerable.Range(0, count)
                .Select(i => new VSTSProject { Id = Guid.NewGuid(), Name = $"Project {i}", DoesNotHaveWorkItems = true }).ToList();
        }

        private Profile CreateProfile()
        {
            return new Profile { Id = Guid.NewGuid(), Members = TeamMemberIds, Repositories = RepositoryIds, Name = "The Profile" };
        }
    }
}
