using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ether.Contracts.Interfaces;
using Ether.Vsts.Dto;
using Ether.Vsts.Handlers.Queries;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Ether.Tests.Handlers.Queries
{
    [TestFixture]
    public class GetVstsDataSourceConfigurationQueryHandlerTests
    {
        private GetVstsDataSourceConfigurationHandler _handler;
        private Mock<IRepository> _repositoryMock;

        [SetUp]
        public void SetUp()
        {
            _repositoryMock = new Mock<IRepository>(MockBehavior.Strict);
            _handler = new GetVstsDataSourceConfigurationHandler(_repositoryMock.Object);
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

            var result = await _handler.Handle(new Vsts.Queries.GetVstsDataSourceConfiguration());

            result.Should().BeNull();
        }

        [Test]
        public async Task ShouldReturnCompleteViewModel()
        {
            var expected = new VstsDataSourceSettings
            {
                Id = Guid.NewGuid(),
                DefaultToken = "FakeToken",
                InstanceName = "Dummy"
            };
            SetupGetSingle(expected);

            var result = await _handler.Handle(new Vsts.Queries.GetVstsDataSourceConfiguration());

            result.Should().NotBeNull();
            result.Id.Should().Be(expected.Id);
            result.DefaultToken.Should().Be(expected.DefaultToken);
            result.InstanceName.Should().Be(expected.InstanceName);
        }

        [Test]
        public void ShouldPropagateAnyExceptions()
        {
            _repositoryMock.Setup(r => r.GetSingleAsync(It.IsAny<Expression<Func<VstsDataSourceSettings, bool>>>()))
                .Throws<NotSupportedException>();

            _handler.Awaiting(h => h.Handle(null))
                .Should().Throw<NotSupportedException>();
        }

        private void SetupGetSingle(VstsDataSourceSettings record)
        {
            _repositoryMock.Setup(r => r.GetSingleAsync(It.Is<Expression<Func<VstsDataSourceSettings, bool>>>(e => VerifyPredicate(e))))
                .ReturnsAsync(record)
                .Verifiable();
        }

        private bool VerifyPredicate(Expression<Func<VstsDataSourceSettings, bool>> predicate)
        {
            var right = ((BinaryExpression)predicate.Body).Right;
            return string.Equals(((ConstantExpression)right).Value, "Vsts");
        }
    }
}
