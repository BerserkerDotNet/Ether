using System;
using AutoMapper;
using Ether.Contracts.Interfaces;
using Ether.Vsts.Commands;
using Ether.Vsts.Dto;

namespace Ether.Vsts.Handlers.Commands
{
    public class SaveRepositoryHandler : SaveHandler<Repository, SaveRepository>
    {
        public SaveRepositoryHandler(IRepository repository, IMapper mapper)
            : base(repository, mapper)
        {
        }

        protected override object GetData(SaveRepository command) => command.Repository;

        protected override void ValidateCommand(SaveRepository command)
        {
            if (command.Repository == null)
            {
                throw new ArgumentNullException(nameof(command.Repository));
            }
        }
    }
}
