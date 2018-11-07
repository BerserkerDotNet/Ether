using System;
using System.Threading.Tasks;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.Vsts.Commands;
using Ether.Vsts.Dto;

namespace Ether.Vsts.Handlers.Commands
{
    public class DeleteProjectHandler : ICommandHandler<DeleteProject>
    {
        private readonly IRepository _repository;

        public DeleteProjectHandler(IRepository repository)
        {
            _repository = repository;
        }

        public async Task Handle(DeleteProject input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            await _repository.DeleteAsync<Project>(input.Id);
        }
    }
}
