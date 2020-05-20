using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Dto;
using Ether.Tests.Extensions;
using Ether.ViewModels;
using Ether.Vsts;
using Ether.Vsts.Dto;
using Ether.Vsts.Handlers.Queries;
using Ether.Vsts.Queries;
using Ether.Vsts.Types;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;

namespace Ether.Tests.Handlers.Queries
{
    [TestFixture]
    public class GetMembersAndRepositoriesOfAllProfilesHandlerTests : BaseHandlerTest
    {
        private GetMembersAndRepositoriesOfAllProfilesHandler _handler;

        [Test]
        public async Task ShouldIncludeMembersAndRepositoriesFromProfiles()
        {
            var data = SetupData();

            var result = await _handler.Handle(new GetMembersAndRepositoriesOfAllProfiles(new Guid[0]));

            var expectedRepositories = data.profiles
                .SelectMany(p => p.Repositories)
                .Distinct()
                .Select(i => data.repositories.Single(r => r.Id == i))
                .Select(r =>
                {
                    var members = data.profiles
                        .Where(p => p.Repositories.Contains(r.Id))
                        .SelectMany(p => p.Members)
                        .Distinct()
                        .Select(i => Mapper.Map<TeamMemberViewModel>(data.members.Single(m => m.Id == i)))
                        .ToArray();
                    var project = data.projects.Single(p => p.Id == r.Project);
                    return new RepositoryInfo
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Members = members,
                        Project = new ProjectInfo
                        {
                            Name = project.Name,
                            IsWorkItemsEnabled = project.IsWorkItemsEnabled,
                            Identity = project.Identity == null ? null : Mapper.Map<IdentityViewModel>(data.identity)
                        }
                    };
                })
                .ToList();

            result.Should().HaveCount(expectedRepositories.Count);
            result.Should().BeEquivalentTo(expectedRepositories);
        }

        [Test]
        public async Task ShouldReturnEmptyIfNoProfiles()
        {
            var data = SetupData();
            SetupMultiple(PredicateValidator, new VstsProfile[0]);

            var result = await _handler.Handle(new GetMembersAndRepositoriesOfAllProfiles(new Guid[0]));

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task ShouldSkipRepositoryIfNoMembers()
        {
            var data = SetupData();
            data.profiles.ElementAt(0).Members = Enumerable.Empty<Guid>();

            var result = await _handler.Handle(new GetMembersAndRepositoriesOfAllProfiles(new Guid[0]));

            result.Should().NotContain(r => !r.Members.Any());
        }

        protected override void Initialize()
        {
            _handler = new GetMembersAndRepositoriesOfAllProfilesHandler(RepositoryMock.Object, Mapper);
        }

        private bool PredicateValidator(Expression exp)
        {
            var lExp = (LambdaExpression)exp;
            var right = ((BinaryExpression)lExp.Body).Right;
            return Equals(((ConstantExpression)right).Value, Constants.VstsType);
        }

#pragma warning disable SA1008 // Opening parenthesis must be spaced correctly
        private (IEnumerable<TeamMember> members,
            IEnumerable<Project> projects,
            IEnumerable<Repository> repositories,
            IEnumerable<VstsProfile> profiles,
            Organization organization,
            Identity identity) SetupData()
#pragma warning restore SA1008 // Opening parenthesis must be spaced correctly
        {
            string type = "Fooooo";

            var identity = Builder<Identity>.CreateNew().Build();
            SetupMultipleWithPredicate(new[] { identity });

            var organization = Builder<Organization>.CreateNew()
                .WithFactory(() => new Organization(type))
                .With(o => o.Identity = identity.Id)
                .Build();
            SetupMultipleWithPredicate(new[] { organization });

            var members = Builder<TeamMember>.CreateListOfSize(10).Build();
            SetupMultipleWithPredicate(members);

            var projects = Builder<Project>.CreateListOfSize(10)
                .All()
                .With(p => p.Identity = Guid.Empty)
                .Random(6)
                .With(p => p.Organization = organization.Id)
                .With(p => p.Identity = identity.Id)
                .Build();
            SetupMultipleWithPredicate(projects);

            var repositories = Builder<Repository>.CreateListOfSize(10)
                .All()
                .With(r => r.Project = projects.PickRandom().Id)
                .Build();
            SetupMultipleWithPredicate(repositories);

            var profiles = Builder<VstsProfile>.CreateListOfSize(3)
                .All()
                .With(p => p.Members = members.PickRandom(1, 3).Select(m => m.Id))
                .With(p => p.Repositories = repositories.PickRandom(1, 3).Select(r => r.Id))
                .Build();
            SetupMultiple(PredicateValidator, profiles);

            return (members, projects, repositories, profiles, organization, identity);
        }
    }
}
