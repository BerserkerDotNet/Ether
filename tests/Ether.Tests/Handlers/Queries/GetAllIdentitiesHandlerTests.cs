using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Dto;
using Ether.Contracts.Interfaces;
using Ether.Core.Config;
using Ether.Core.Types.Handlers.Queries;
using Ether.Core.Types.Queries;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Ether.Tests.Handlers.Queries
{
    [TestFixture]
    public class GetAllIdentitiesHandlerTests
    {
        private GetAllIdentitiesHandler _handler;
        private Mock<IRepository> _repositoryMock;
        private IMapper _mapper;

        [SetUp]
        public void SetUp()
        {
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new CoreMappingProfile());
            });
            _mapper = mappingConfig.CreateMapper();

            _repositoryMock = new Mock<IRepository>(MockBehavior.Strict);
            _handler = new GetAllIdentitiesHandler(_repositoryMock.Object, _mapper);
        }

        [Test]
        public void ShouldNotThrowExceptionIfQueryIsNull()
        {
            _repositoryMock.Setup(r => r.GetAllAsync<Identity>())
                .ReturnsAsync(Enumerable.Empty<Identity>());

            _handler.Awaiting(h => h.Handle(null))
                .Should().NotThrow();
        }

        [Test]
        public async Task ShouldReturnAllQueries()
        {
            var expectedIdentities = Enumerable.Range(1, 5)
                .Select(i => new Identity { Id = Guid.NewGuid(), Name = $"Identity {i}", Token = $"Token {i}", ExpirationDate = DateTime.UtcNow.AddDays(i) })
                .ToArray();
            _repositoryMock.Setup(r => r.GetAllAsync<Identity>())
                .ReturnsAsync(expectedIdentities)
                .Verifiable();

            var result = await _handler.Handle(new GetAllIdentities());
            _mapper.Map<IEnumerable<Identity>>(result).Should().BeEquivalentTo(expectedIdentities);
            _repositoryMock.VerifyAll();
        }
    }
}
