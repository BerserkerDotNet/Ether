using System;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Dto;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;

namespace Ether.Vsts.Handlers.Commands
{
    public abstract class SaveHandler<TModel, TData, TCommand> : ICommandHandler<TCommand>
        where TModel : ViewModelWithId
        where TCommand : ICommand
        where TData : BaseDto
    {
        public SaveHandler(IRepository repository, IMapper mapper)
        {
            Repository = repository;
            Mapper = mapper;
        }

        protected IRepository Repository { get; private set; }

        protected IMapper Mapper { get; private set; }

        public async Task Handle(TCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            ValidateCommand(command);

            var data = await FixViewModel(command);
            var dto = Mapper.Map<TData>(data);
            await Repository.CreateOrUpdateAsync(dto);
        }

        protected abstract void ValidateCommand(TCommand command);

        protected abstract Task<TModel> FixViewModel(TCommand command);
    }
}
