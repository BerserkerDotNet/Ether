using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Dto;
using Ether.Contracts.Interfaces;
using Ether.Core.Types.Commands;
using Ether.ViewModels;
using Ether.Vsts;
using Ether.Vsts.Commands;
using Ether.Vsts.Dto;
using Ether.Vsts.Handlers.Commands;
using FluentAssertions;
using NUnit.Framework;

namespace Ether.Tests.Handlers.Commands
{
    [TestFixture]
    public class SaveOrganizationHandlerTests : BaseHandlerTest
    {
        private SaveOrganizationHandler _handler;

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
            var expectedOrganization = new OrganizationViewModel
            {
                Id = Guid.NewGuid(),
                Identity = Guid.NewGuid(),
                Name = "Fooooo",
                Type = "Fooooo"
            };

            SetupCreateOrUpdate<Organization, OrganizationViewModel>(expectedOrganization);

            await _handler.Handle(new SaveOrganization { Organization = expectedOrganization });

            RepositoryMock.VerifyAll();
        }

        protected override void Initialize()
        {
            _handler = new SaveOrganizationHandler(RepositoryMock.Object, Mapper);
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
