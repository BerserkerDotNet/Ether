using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Interfaces;
using Ether.ViewModels;
using Ether.Vsts;
using Ether.Vsts.Dto;
using Ether.Vsts.Handlers.Queries;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Ether.Tests.Handlers.Queries
{
    [TestFixture]
    public class GetOrganizationByIdQueryHandlerTests : BaseHandlerTest
    {
        private GetOrganizationByIdHandler _handler;

        [Test]
        public void ShouldThrowNullExceptionIfQueryIsNull()
        {
            VstsOrganization nullOrganization = null;

            SetupSingle(nullOrganization);

            _handler.Awaiting(h => h.Handle(null))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task ShouldReturnNullIfRecordNotFound()
        {
            VstsOrganization nullOrganization = null;

            SetupSingle(nullOrganization);

            var result = await _handler.Handle(new Vsts.Queries.GetOrganizationById(default));

            result.Should().BeNull();
        }

        [Test]
        public async Task ShouldReturnCompleteViewModel()
        {
            var expectedOrganization = new VstsOrganization
            {
                Id = Guid.NewGuid(),
                Identity = Guid.NewGuid(),
                Name = "Dummy",
                Type = "Dummy"
            };

            SetupSingle(expectedOrganization);

            var result = await _handler.Handle(new Vsts.Queries.GetOrganizationById(expectedOrganization.Id));

            result.Should().NotBeNull();
            result.Id.Should().Be(expectedOrganization.Id);
            result.Identity.Should().Be(expectedOrganization.Identity);
            result.Name.Should().Be(expectedOrganization.Name);
            result.Type.Should().Be(expectedOrganization.Type);
        }

        protected override void Initialize()
        {
            _handler = new GetOrganizationByIdHandler(RepositoryMock.Object, Mapper);
        }
    }
}
