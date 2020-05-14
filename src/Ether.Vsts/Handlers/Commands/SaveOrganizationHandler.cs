using System;
using System.Threading.Tasks;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.Vsts.Commands;
using Ether.Vsts.Dto;

namespace Ether.Vsts.Handlers.Commands
{
    public class SaveOrganizationHandler : ICommandHandler<SaveOrganization>
    {
        private readonly IRepository _repository;

        public SaveOrganizationHandler(IRepository repository)
        {
            _repository = repository;
        }

        public async Task Handle(SaveOrganization input)
        {
            if (input == null || input.Organization == null)
            {
                throw new ArgumentNullException(nameof(input.Organization));
            }

            var config = input.Organization;
            await _repository.CreateOrUpdateIfAsync(i => i.Type == Constants.VstsType, new Organization
            {
                Id = config.Id,
                Identity = config.Identity,
                Name = config.Name
            });
        }
    }
}
