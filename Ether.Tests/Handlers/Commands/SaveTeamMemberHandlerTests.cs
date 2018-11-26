using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ether.ViewModels;
using Ether.Vsts.Commands;
using Ether.Vsts.Dto;
using Ether.Vsts.Exceptions;
using Ether.Vsts.Handlers.Commands;
using Ether.Vsts.Interfaces;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using VSTS.Net.Interfaces;

namespace Ether.Tests.Handlers.Commands
{
    [TestFixture]
    public class SaveTeamMemberHandlerTests : BaseHandlerTest
    {
        private SaveTeamMemberHandler _handler;
        private Mock<IVstsIdentityClient> _vstsIdentityClientMock;

        [Test]
        public async Task ShouldFetchIdentityIdFromVsts()
        {
            const string expectedEmail = "foo@bla.com";
            var vm = Builder<TeamMemberViewModel>.CreateNew()
                .With(m => m.Email = expectedEmail)
                .With(m => m.Id = Guid.Empty)
                .Build();
            var identities = Builder<VSTS.Net.Models.Identity.Identity>.CreateListOfSize(3)
                .All()
                .With(i => i.IsActive = true)
                .Build();
            _vstsIdentityClientMock.Setup(m => m.GetIdentitiesAsync(expectedEmail, true, "General", default(CancellationToken)))
                .ReturnsAsync(identities)
                .Verifiable();

            SetupCreateOrUpdate<TeamMember>(m => m.Id == identities.ElementAt(0).Id);

            await _handler.Handle(new SaveTeamMember { TeamMember = vm });

            RepositoryMock.Verify();
            _vstsIdentityClientMock.Verify();
        }

        [Test]
        public async Task ShouldOverrideTeamMemberId()
        {
            const string expectedEmail = "foo@bla.com";
            var vm = Builder<TeamMemberViewModel>.CreateNew()
                .With(m => m.Email = expectedEmail)
                .With(m => m.Id = Guid.NewGuid())
                .Build();
            var identities = Builder<VSTS.Net.Models.Identity.Identity>.CreateListOfSize(3)
                .All()
                .With(i => i.IsActive = true)
                .Build();
            _vstsIdentityClientMock.Setup(m => m.GetIdentitiesAsync(expectedEmail, true, "General", default(CancellationToken)))
                .ReturnsAsync(identities)
                .Verifiable();

            SetupCreateOrUpdate<TeamMember>(m => m.Id == identities.ElementAt(0).Id);

            await _handler.Handle(new SaveTeamMember { TeamMember = vm });

            RepositoryMock.Verify();
            _vstsIdentityClientMock.Verify();
        }

        [Test]
        public void ShouldThrowExceptionIfNoIdentitiesFound()
        {
            const string expectedEmail = "foo@bla.com";
            var vm = Builder<TeamMemberViewModel>.CreateNew()
                .With(m => m.Email = expectedEmail)
                .With(m => m.Id = Guid.NewGuid())
                .Build();
            _vstsIdentityClientMock.Setup(m => m.GetIdentitiesAsync(expectedEmail, true, "General", default(CancellationToken)))
                .ReturnsAsync(Enumerable.Empty<VSTS.Net.Models.Identity.Identity>())
                .Verifiable();

            _handler.Awaiting(h => h.Handle(new SaveTeamMember { TeamMember = vm }))
                .Should().Throw<IdentityNotFoundException>();

            _vstsIdentityClientMock.Verify();
        }

        protected override void Initialize()
        {
            _vstsIdentityClientMock = new Mock<IVstsIdentityClient>(MockBehavior.Strict);
            var factoryMock = new Mock<IVstsClientFactory>();
            factoryMock.Setup(f => f.GetIdentityClient(null))
                .ReturnsAsync(_vstsIdentityClientMock.Object);

            _handler = new SaveTeamMemberHandler(RepositoryMock.Object, factoryMock.Object, Mapper);
        }
    }
}
