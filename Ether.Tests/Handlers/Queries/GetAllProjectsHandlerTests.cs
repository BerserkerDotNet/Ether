using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ether.Vsts;
using Ether.Vsts.Dto;
using Ether.Vsts.Handlers.Queries;
using Ether.Vsts.Queries;
using FluentAssertions;
using NUnit.Framework;

namespace Ether.Tests.Handlers.Queries
{
    [TestFixture]
    public class GetAllProjectsHandlerTests : BaseHandlerTest
    {
        private GetAllProjectsHandler _handler;

        [Test]
        public void ShouldNotThrowExceptionIfQueryIsNull()
        {
            SetupMultiple(Enumerable.Empty<Project>());

            _handler.Awaiting(h => h.Handle(null))
                .Should().NotThrow();
        }

        [Test]
        public async Task ShouldReturnAllQueries()
        {
            var expected = Enumerable.Range(1, 5)
                .Select(i => new Project { Id = Guid.NewGuid(), Name = $"Project {i}", IsWorkItemsEnabled = true, Identity = Guid.NewGuid() })
                .ToArray();

            SetupMultiple(expected);

            var result = await _handler.Handle(new GetAllProjects());

            Mapper.Map<IEnumerable<Project>>(result).Should().BeEquivalentTo(expected);
            RepositoryMock.VerifyAll();
        }

        protected override void Initialize()
        {
            _handler = new GetAllProjectsHandler(RepositoryMock.Object, Mapper);
        }
    }

    [TestFixture]
    public class GetAllProfilesHandlerTests : BaseHandlerTest
    {
        private GetAllProfilesHandler _handler;

        [Test]
        public void ShouldNotThrowExceptionIfQueryIsNull()
        {
            SetupMultiple(_ => true, Enumerable.Empty<VstsProfile>());

            _handler.Awaiting(h => h.Handle(null))
                .Should().NotThrow();
        }

        [Test]
        public async Task ShouldReturnAllQueries()
        {
            var expected = Enumerable.Range(1, 5)
                .Select(i => new VstsProfile { Id = Guid.NewGuid(), Name = $"Profile {i}", Members = new[] { Guid.NewGuid() }, Repositories = new[] { Guid.NewGuid() } })
                .ToArray();

            SetupMultiple(PredicateValidator, expected);

            var result = await _handler.Handle(new GetAllProfiles());

            Mapper.Map<IEnumerable<VstsProfile>>(result).Should().BeEquivalentTo(expected);
            RepositoryMock.VerifyAll();
        }

        protected override void Initialize()
        {
            _handler = new GetAllProfilesHandler(RepositoryMock.Object, Mapper);
        }

        private bool PredicateValidator(Expression exp)
        {
            var lExp = (LambdaExpression)exp;
            var right = ((BinaryExpression)lExp.Body).Right;
            return Equals(((ConstantExpression)right).Value, Constants.VstsType);
        }
    }
}
