using System;
using System.Threading.Tasks;
using Ether.Contracts.Dto;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.Core.Types.Commands;

namespace Ether.Core.Types.Handlers.Commands
{
    public class DeleteIdentityHandler : ICommandHandler<DeleteIdentity>
    {
        private readonly IRepository _repository;

        public DeleteIdentityHandler(IRepository repository)
        {
            _repository = repository;
        }

        public async Task Handle(DeleteIdentity input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            await _repository.DeleteAsync<Identity>(input.Id);
        }
    }
}
