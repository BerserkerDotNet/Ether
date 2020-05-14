using System;
using System.Threading.Tasks;
using Ether.Contracts.Dto;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.Core.Types.Commands;

namespace Ether.Core.Types.Handlers.Commands
{
    public class DeleteOrganizationHandler : ICommandHandler<DeleteOrganization>
    {
        private readonly IRepository _repository;

        public DeleteOrganizationHandler(IRepository repository)
        {
            _repository = repository;
        }

        public async Task Handle(DeleteOrganization input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            await _repository.DeleteAsync<Organization>(input.Id);
        }
    }
}
