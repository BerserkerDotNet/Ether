using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            var expectedIdentities = Enumerable.Range(1, 5)
                .Select(i => new Project { Id = Guid.NewGuid(), Name = $"Project {i}", IsWorkItemsEnabled = true, Identity = Guid.NewGuid() })
                .ToArray();

            SetupMultiple(expectedIdentities);

            var result = await _handler.Handle(new GetAllProjects());

            Mapper.Map<IEnumerable<Project>>(result).Should().BeEquivalentTo(expectedIdentities);
            RepositoryMock.VerifyAll();
        }

        protected override void Initialize()
        {
            _handler = new GetAllProjectsHandler(RepositoryMock.Object, Mapper);
        }
    }
}
