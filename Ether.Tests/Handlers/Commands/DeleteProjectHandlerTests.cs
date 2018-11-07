using System;
using System.Threading.Tasks;
using Ether.Contracts.Dto;
using Ether.Vsts.Commands;
using Ether.Vsts.Dto;
using Ether.Vsts.Handlers.Commands;
using FluentAssertions;
using NUnit.Framework;

namespace Ether.Tests.Handlers.Commands
{
    public class DeleteProjectHandlerTests : BaseHandlerTest
    {
        private DeleteProjectHandler _handler;

        [Test]
        public void ShouldThrowExceptionIfCommandIsNull()
        {
            _handler.Awaiting(h => h.Handle(null))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void ShouldNotThrowExceptionIfIdIsEmpty()
        {
            SetupDelete<Project>();

            _handler.Awaiting(h => h.Handle(new DeleteProject { Id = Guid.Empty }))
                .Should().NotThrow();
        }

        [Test]
        public async Task ShouldDeleteRecord()
        {
            SetupDelete<Project>();

            await _handler.Handle(new DeleteProject { Id = Guid.NewGuid() });

            RepositoryMock.VerifyAll();
        }

        protected override void Initialize()
        {
            _handler = new DeleteProjectHandler(RepositoryMock.Object);
        }
    }
}
