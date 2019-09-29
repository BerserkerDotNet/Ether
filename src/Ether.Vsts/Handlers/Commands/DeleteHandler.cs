using System;
using System.Threading.Tasks;
using Ether.Contracts.Dto;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;

namespace Ether.Vsts.Handlers.Commands
{
    public abstract class DeleteHandler<TData, TCommand> : ICommandHandler<TCommand>
        where TCommand : ICommand
        where TData : BaseDto
    {
        private readonly IRepository _repository;

        public DeleteHandler(IRepository repository)
        {
            _repository = repository;
        }

        public async Task Handle(TCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            await _repository.DeleteAsync<TData>(GetId(command));
        }

        protected abstract Guid GetId(TCommand command);
    }
}
