using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Interfaces;
using Ether.Core.Types.Commands;
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
        private Mock<IMapper> _mapperMock;
        private SaveOrganizationHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _repositorMock = new Mock<IRepository>(MockBehavior.Strict);
            _mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            _handler = new SaveOrganizationHandler(_repositorMock.Object, _mapperMock.Object);
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
            _repositorMock.Setup(r => r.CreateOrUpdateIfAsync(It.Is<Expression<Func<VstsOrganization, bool>>>(e => CheckCriteria(e)), It.Is<VstsOrganization>(s => CheckDtoRecord(config, s))))
                .ReturnsAsync(true)
                .Verifiable();

            await _handler.Handle(new SaveOrganization { Organization = config });

            _repositorMock.VerifyAll();
        }

        private bool CheckDtoRecord(OrganizationViewModel model, VstsOrganization record)
        {
            return model.Id == record.Id &&
                string.Equals(model.Name, record.Name) &&
                string.Equals(model.Identity, record.Identity);
        }

        private bool CheckCriteria(Expression<Func<VstsOrganization, bool>> criteria)
        {
            var right = ((BinaryExpression)criteria.Body).Right;
            return string.Equals(((ConstantExpression)right).Value, Constants.VstsType);
        }
    }
}
