using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ether.Contracts.Interfaces;
using Ether.ViewModels;
using Ether.Vsts.Commands;
using Ether.Vsts.Dto;
using Ether.Vsts.Handlers.Commands;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Ether.Tests.Handlers.Commands
{
    [TestFixture]
    public class SaveVstsDataSourceConfigurationHandlerTests
    {
        private Mock<IRepository> _repositorMock;
        private SaveVstsDataSourceConfigurationHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _repositorMock = new Mock<IRepository>(MockBehavior.Strict);
            _handler = new SaveVstsDataSourceConfigurationHandler(_repositorMock.Object);
        }

        [Test]
        public void ShouldThrowExceptionIfCommandIsNull()
        {
            _handler.Awaiting(h => h.Handle(null))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void ShouldThrowExceptionIfConfigurationIsNull()
        {
            _handler.Awaiting(h => h.Handle(new SaveVstsDataSourceConfiguration { Configuration = null }))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task ShouldSaveConfiguration()
        {
            var config = new VstsDataSourceViewModel
            {
                Id = Guid.NewGuid(),
                DefaultToken = "Secret",
                InstanceName = "Fooooo"
            };
            _repositorMock.Setup(r => r.CreateOrUpdateIfAsync(It.Is<Expression<Func<VstsDataSourceSettings, bool>>>(e => CheckCriteria(e)), It.Is<VstsDataSourceSettings>(s => CheckDtoRecord(config, s))))
                .ReturnsAsync(true)
                .Verifiable();

            await _handler.Handle(new SaveVstsDataSourceConfiguration { Configuration = config });

            _repositorMock.VerifyAll();
        }

        private bool CheckDtoRecord(VstsDataSourceViewModel model, VstsDataSourceSettings record)
        {
            return model.Id == record.Id &&
                string.Equals(model.InstanceName, record.InstanceName) &&
                string.Equals(model.DefaultToken, record.DefaultToken);
        }

        private bool CheckCriteria(Expression<Func<VstsDataSourceSettings, bool>> criteria)
        {
            var right = ((BinaryExpression)criteria.Body).Right;
            return string.Equals(((ConstantExpression)right).Value, "Vsts");
        }
    }
}
