using System;
using System.Threading.Tasks;
using Ether.Contracts.Dto;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;
using Ether.Vsts.Commands;
using Ether.Vsts.Dto;
using Ether.Vsts.Handlers.Commands;
using FluentAssertions;
using NUnit.Framework;

namespace Ether.Tests.Handlers.Commands
{
    public class DeleteHandlerTests : BaseHandlerTest
    {
        private DeleteFakeCommandHandler _handler;

        [Test]
        public void ShouldThrowExceptionIfCommandIsNull()
        {
            _handler.Awaiting(h => h.Handle(null))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void ShouldNotThrowExceptionIfIdIsEmpty()
        {
            SetupDelete<Fake>();

            _handler.Awaiting(h => h.Handle(new DeleteFakeCommand { Id = Guid.Empty }))
                .Should().NotThrow();
        }

        [Test]
        public async Task ShouldDeleteRecord()
        {
            SetupDelete<Fake>();

            await _handler.Handle(new DeleteFakeCommand { Id = Guid.NewGuid() });

            RepositoryMock.VerifyAll();
        }

        protected override void Initialize()
        {
            _handler = new DeleteFakeCommandHandler(RepositoryMock.Object);
        }

        private class Fake : BaseDto
        {
        }

        private class FakeViewModel : ViewModelWithId
        {
        }

        private class DeleteFakeCommand : ICommand
        {
            public Guid Id { get; set; }
        }

        private class DeleteFakeCommandHandler : DeleteHandler<Fake, DeleteFakeCommand>
        {
            public DeleteFakeCommandHandler(IRepository repository)
                : base(repository)
            {
            }

            protected override Guid GetId(DeleteFakeCommand command) => command.Id;
        }
    }
}
