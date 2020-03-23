using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ether.ViewModels;
using Ether.Vsts.Commands;
using Ether.Vsts.Dto;
using Ether.Vsts.Handlers.Commands;
using FizzWare.NBuilder;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Ether.Tests.Handlers.Commands
{
    [TestFixture]
    public class DeleteWorkItemsHandlerTests : BaseHandlerTest
    {
        private DeleteWorkItemsHandler _handler;

        [Test]
        public void ShouldThrowExceptionIfIdsIsNull()
        {
            _handler.Awaiting(h => h.Handle(new DeleteWorkItems(null)))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task ShouldNotDoAnythingIfNoIds()
        {
            await _handler.Handle(new DeleteWorkItems(Enumerable.Empty<int>()));

            RepositoryMock.Verify(r => r.DeleteAsync<WorkItem>(It.IsAny<Guid>()), Times.Never);
        }

        protected override void Initialize()
        {
            _handler = new DeleteWorkItemsHandler(RepositoryMock.Object);
        }
    }
}
