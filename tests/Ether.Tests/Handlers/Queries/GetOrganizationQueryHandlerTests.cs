using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Interfaces;
using Ether.Vsts;
using Ether.Vsts.Dto;
using Ether.Vsts.Handlers.Queries;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Ether.Tests.Handlers.Queries
{
    [TestFixture]
    public class GetOrganizationQueryHandlerTests
    {
        private GetOrganizationByIdHandler _handler;
        private Mock<IRepository> _repositoryMock;
        private Mock<IMapper> _mapperMock;

        [SetUp]
        public void SetUp()
        {
            _repositoryMock = new Mock<IRepository>(MockBehavior.Strict);
            _mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            _handler = new GetOrganizationByIdHandler(_repositoryMock.Object, _mapperMock.Object);
        }

        [Test]
        public void ShouldNotThrowExceptionIfQueryIsNull()
        {
            SetupGetSingle(null);

            _handler.Awaiting(h => h.Handle(null))
                .Should().NotThrow();
        }

        [Test]
        public async Task ShouldReturnNullIfRecordNotFound()
        {
            SetupGetSingle(null);

            var result = await _handler.Handle(new Vsts.Queries.GetOrganizationById(default));

            result.Should().BeNull();
        }

        [Test]
        public async Task ShouldReturnCompleteViewModel()
        {
            var expected = new Organization
            {
                Id = Guid.NewGuid(),
                Identity = Guid.NewGuid(),
                Name = "Dummy"
            };
            SetupGetSingle(expected);

            var result = await _handler.Handle(new Vsts.Queries.GetOrganizationById(expected.Id));

            result.Should().NotBeNull();
            result.Id.Should().Be(expected.Id);
            result.Identity.Should().Be(expected.Identity);
            result.Name.Should().Be(expected.Name);
        }

        [Test]
        public void ShouldPropagateAnyExceptions()
        {
            _repositoryMock.Setup(r => r.GetSingleAsync(It.IsAny<Expression<Func<Organization, bool>>>()))
                .Throws<NotSupportedException>();

            _handler.Awaiting(h => h.Handle(null))
                .Should().Throw<NotSupportedException>();
        }

        private void SetupGetSingle(Organization record)
        {
            _repositoryMock.Setup(r => r.GetSingleAsync(It.Is<Expression<Func<Organization, bool>>>(e => VerifyPredicate(e))))
                .ReturnsAsync(record)
                .Verifiable();
        }

        private bool VerifyPredicate(Expression<Func<Organization, bool>> predicate)
        {
            var right = ((BinaryExpression)predicate.Body).Right;
            return string.Equals(((ConstantExpression)right).Value, Constants.VstsType);
        }
    }
}
