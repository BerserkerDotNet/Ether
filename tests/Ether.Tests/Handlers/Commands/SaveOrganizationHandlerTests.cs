using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ether.Contracts.Interfaces;
using Ether.ViewModels;
using Ether.Vsts;
using Ether.Vsts.Commands;
using Ether.Vsts.Dto;
using Ether.Vsts.Handlers.Commands;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Ether.Tests.Handlers.Commands
{
    [TestFixture]
    public class SaveOrganizationHandlerTests
    {
        private Mock<IRepository> _repositorMock;
        private SaveOrganizationHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _repositorMock = new Mock<IRepository>(MockBehavior.Strict);
            _handler = new SaveOrganizationHandler(_repositorMock.Object);
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
            _handler.Awaiting(h => h.Handle(new SaveOrganization { Organization = null }))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task ShouldSaveConfiguration()
        {
            var config = new OrganizationViewModel
            {
                Id = Guid.NewGuid(),
                Identity = Guid.NewGuid(),
                Name = "Fooooo"
            };
            _repositorMock.Setup(r => r.CreateOrUpdateIfAsync(It.Is<Expression<Func<Organization, bool>>>(e => CheckCriteria(e)), It.Is<Organization>(s => CheckDtoRecord(config, s))))
                .ReturnsAsync(true)
                .Verifiable();

            await _handler.Handle(new SaveOrganization { Organization = config });

            _repositorMock.VerifyAll();
        }

        private bool CheckDtoRecord(OrganizationViewModel model, Organization record)
        {
            return model.Id == record.Id &&
                string.Equals(model.Name, record.Name) &&
                string.Equals(model.Identity, record.Identity);
        }

        private bool CheckCriteria(Expression<Func<Organization, bool>> criteria)
        {
            var right = ((BinaryExpression)criteria.Body).Right;
            return string.Equals(((ConstantExpression)right).Value, Constants.VstsType);
        }
    }
}
