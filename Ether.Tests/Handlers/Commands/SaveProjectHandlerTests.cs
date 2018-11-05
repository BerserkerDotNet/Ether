using System;
using System.Threading.Tasks;
using Ether.ViewModels;
using Ether.Vsts.Commands;
using Ether.Vsts.Dto;
using Ether.Vsts.Handlers.Commands;
using FluentAssertions;
using NUnit.Framework;

namespace Ether.Tests.Handlers.Commands
{
    public class SaveProjectHandlerTests : BaseHandlerTest
    {
        private SaveProjectHandler _handler;

        [Test]
        public void ShouldThrowExceptionIfCommandIsNull()
        {
            _handler.Awaiting(h => h.Handle(null))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void ShouldThrowExceptionIfModelIsNull()
        {
            _handler.Awaiting(h => h.Handle(new SaveProject { Project = null }))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task ShouldSaveIdentity()
        {
            var expectedProject = new VstsProjectViewModel
            {
                Id = Guid.NewGuid(),
                Name = "New proejct",
                IsWorkItemsEnabled = true,
                Identity = Guid.NewGuid()
            };

            SetupCreateOrUpdate<Project, VstsProjectViewModel>(expectedProject);

            await _handler.Handle(new SaveProject
            {
                Project = expectedProject
            });

            RepositoryMock.VerifyAll();
        }

        protected override void Initialize()
        {
            _handler = new SaveProjectHandler(RepositoryMock.Object, Mapper);
        }
    }
}
