using System;
using System.Threading.Tasks;
using Ether.Contracts.Dto;
using Ether.Core.Types.Commands;
using Ether.Core.Types.Handlers.Commands;
using Ether.ViewModels;
using FluentAssertions;
using NUnit.Framework;

namespace Ether.Tests.Handlers.Commands
{
    [TestFixture]
    public class SaveIdentityHandlerTests : BaseHandlerTest
    {
        private SaveIdentityHandler _handler;

        [Test]
        public void ShouldThrowExceptionIfCommandIsNull()
        {
            _handler.Awaiting(h => h.Handle(null))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void ShouldThrowExceptionIfModelIsNull()
        {
            _handler.Awaiting(h => h.Handle(new SaveIdentity { Identity = null }))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task ShouldSaveIdentity()
        {
            var expectedIdentity = new IdentityViewModel
            {
                Id = Guid.NewGuid(),
                Name = "New identity",
                Token = "Secret",
                ExpirationDate = DateTime.UtcNow
            };

            SetupCreateOrUpdate<Identity, IdentityViewModel>(expectedIdentity);

            await _handler.Handle(new SaveIdentity
            {
                Identity = expectedIdentity
            });

            RepositoryMock.VerifyAll();
        }

        protected override void Initialize()
        {
            _handler = new SaveIdentityHandler(RepositoryMock.Object, Mapper);
        }
    }
}
