using System;
using System.Threading.Tasks;
using Ether.Contracts.Dto;
using Ether.Core.Types.Commands;
using Ether.Core.Types.Handlers.Commands;
using FluentAssertions;
using NUnit.Framework;

namespace Ether.Tests.Handlers.Commands
{
    public class DeleteIdentityHandlerTests : BaseHandlerTest
    {
        private DeleteIdentityHandler _handler;

        [Test]
        public void ShouldThrowExceptionIfCommandIsNull()
        {
            _handler.Awaiting(h => h.Handle(null))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void ShouldNotThrowExceptionIfIdIsEmpty()
        {
            SetupDelete<Identity>();

            _handler.Awaiting(h => h.Handle(new DeleteIdentity { Id = Guid.Empty }))
                .Should().NotThrow();
        }

        [Test]
        public async Task ShouldDeleteRecord()
        {
            SetupDelete<Identity>();

            await _handler.Handle(new DeleteIdentity { Id = Guid.NewGuid() });

            RepositoryMock.VerifyAll();
        }

        protected override void Initialize()
        {
            _handler = new DeleteIdentityHandler(RepositoryMock.Object);
        }
    }
}
