using System;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Dto;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;

namespace Ether.Vsts.Handlers.Commands
{
    public abstract class SaveHandler<TData, TCommand> : ICommandHandler<TCommand>
        where TCommand : ICommand
        where TData : BaseDto
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;

        public SaveHandler(IRepository repository, IMapper mapper)
        {
            this._repository = repository;
            this._mapper = mapper;
        }

        public async Task Handle(TCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            ValidateCommand(command);

            var dto = _mapper.Map<TData>(GetData(command));
            await _repository.CreateOrUpdateAsync(dto);
        }

        protected abstract void ValidateCommand(TCommand command);

        protected abstract object GetData(TCommand command);
    }
}
