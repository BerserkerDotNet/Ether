using Ether.Core.Configuration;
using Ether.Core.Interfaces;
using Ether.Core.Models.DTO;
using Ether.Core.Models.DTO.Reports;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ether.Tests
{
    public static class Common
    {
        public static void SetupConfiguration(Mock<IOptions<VSTSConfiguration>> mock, string token = "Foo", string instance = "Bar")
        {
            mock.SetupGet(c => c.Value)
                .Returns(new VSTSConfiguration { AccessToken = token, InstanceName = instance })
                .Verifiable();
        }

        public static (Profile profile, TeamMember[] members, VSTSProject[] projects, VSTSRepository[] repositories)
            SetupDataForBaseReporter(Mock<IRepository> repositoryMock, int membersCount = 3, int takeMembers = 2, int projectsCount = 2, int repoCount = 2)
        {
            var members = GetMembers(membersCount);
            var projects = new[] { new VSTSProject { Id = Guid.NewGuid() }, new VSTSProject { Id = Guid.NewGuid() } };
            var repositories = new[] { new VSTSRepository { Id = Guid.NewGuid(), Project = projects[0].Id }, new VSTSRepository { Id = Guid.NewGuid(), Project = projects[0].Id } };
            var profile = new Profile
            {
                Id = Guid.NewGuid(),
                Name = "Foo",
                Members = members.Select(m => m.Id).Take(takeMembers),
                Repositories = repositories.Select(r => r.Id)
            };

            repositoryMock.Setup(r => r.GetSingleAsync(It.IsAny<Expression<Func<Profile, bool>>>()))
                .Returns(Task.FromResult(profile));
            repositoryMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<TeamMember, bool>>>()))
                .Returns<Expression<Func<TeamMember, bool>>>(e => Task.FromResult(members.Where(e.Compile())));
            repositoryMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<VSTSRepository, bool>>>()))
                .Returns<Expression<Func<VSTSRepository, bool>>>(e => Task.FromResult(repositories.Where(e.Compile())));
            repositoryMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<VSTSProject, bool>>>()))
                .Returns<Expression<Func<VSTSProject, bool>>>(e => Task.FromResult(projects.Where(e.Compile())));
            repositoryMock.Setup(r => r.CreateAsync(It.IsAny<ReportResult>()))
                .Returns(Task.FromResult(true));

            return (profile, members, projects, repositories);
        }

        private static TeamMember[] GetMembers(int count)
        {
            return Enumerable.Range(1, count).Select(i => new TeamMember
            {
                Id = Guid.NewGuid(),
                DisplayName = $"Member {i}",
                Email = $"foo{i}@bar.com",
            }).ToArray();
        }
    }
}
