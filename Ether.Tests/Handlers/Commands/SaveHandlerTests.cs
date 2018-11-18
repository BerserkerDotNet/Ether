using System;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Dto;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;
using Ether.Vsts.Handlers.Commands;
using FluentAssertions;
using NUnit.Framework;

namespace Ether.Tests.Handlers.Commands
{
    [TestFixture]
    public class SaveHandlerTests : BaseHandlerTest
    {
        private SaveFakeCommandHandler _handler;

        [Test]
        public void ShouldThrowExceptionIfCommandIsNull()
        {
            _handler.Awaiting(h => h.Handle(null))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void ShouldThrowExceptionIfModelIsNull()
        {
            _handler.Awaiting(h => h.Handle(new SaveFakeCommand { Fake = null }))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task ShouldSaveModel()
        {
            var expected = new FakeViewModel
            {
                Id = Guid.NewGuid()
            };

            SetupCreateOrUpdate<Fake, FakeViewModel>(expected);

            await _handler.Handle(new SaveFakeCommand
            {
                Fake = expected
            });

            RepositoryMock.VerifyAll();
        }

        protected override void Initialize()
        {
            _handler = new SaveFakeCommandHandler(RepositoryMock.Object, Mapper);
        }

        protected override void InitializeMappings(IMapperConfigurationExpression config)
        {
            config.CreateMap<Fake, FakeViewModel>();
            config.CreateMap<FakeViewModel, Fake>();
            base.InitializeMappings(config);
        }

        private class Fake : BaseDto
        {
        }

        private class FakeViewModel : ViewModelWithId
        {
        }

        private class SaveFakeCommand : ICommand
        {
            public FakeViewModel Fake { get; set; }
        }

        private class SaveFakeCommandHandler : SaveHandler<FakeViewModel, Fake, SaveFakeCommand>
        {
            public SaveFakeCommandHandler(IRepository repository, IMapper mapper)
                : base(repository, mapper)
            {
            }

            protected override Task<FakeViewModel> GetData(SaveFakeCommand command) => Task.FromResult(command.Fake);

            protected override void ValidateCommand(SaveFakeCommand command)
            {
                if (command.Fake == null)
                {
                    throw new ArgumentNullException(nameof(command.Fake));
                }
            }
        }
    }
}
