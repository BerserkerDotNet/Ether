using System;
using System.Threading.Tasks;
using Ether.Contracts.Dto;
using Ether.Core.Types.Handlers.Queries;
using Ether.Core.Types.Queries;
using ExpectedObjects;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Ether.Tests.Handlers.Queries
{
    [TestFixture]
    public class GetIdentityByIdHandlerTests : BaseHandlerTest
    {
        private GetIdentityByIdHandler _handler;

        [Test]
        public void ShouldThrowExceptionIfQueryIsNull()
        {
            _handler.Awaiting(h => h.Handle(null))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task ShouldReturnNullIfEmptyGuid()
        {
            var result = await _handler.Handle(new GetIdentityById { Id = Guid.Empty });

            result.Should().BeNull();
            RepositoryMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task ShouldReturnNullIfNoRecord()
        {
            SetupNull<Identity>();

            var result = await _handler.Handle(new GetIdentityById { Id = Guid.NewGuid() });

            result.Should().BeNull();
            RepositoryMock.VerifyAll();
        }

        [Test]
        public async Task ShouldReturnCorrectViewModel()
        {
            var expectedIdentity = new Identity
            {
                Id = Guid.NewGuid(),
                Name = "Fake identity",
                Token = "Secret token",
                ExpirationDate = DateTime.UtcNow.AddDays(30)
            };
            SetupSingle(expectedIdentity);

            var result = await _handler.Handle(new GetIdentityById { Id = expectedIdentity.Id });

            result.Should().NotBeNull();
            result.ToExpectedObject().ShouldMatch(expectedIdentity);
            RepositoryMock.VerifyAll();
        }

        protected override void Initialize()
        {
            _handler = new GetIdentityByIdHandler(RepositoryMock.Object, Mapper);
        }
    }
}
