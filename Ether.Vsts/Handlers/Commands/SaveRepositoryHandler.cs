using System;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Interfaces;
using Ether.ViewModels;
using Ether.Vsts.Commands;
using Ether.Vsts.Dto;

namespace Ether.Vsts.Handlers.Commands
{
    public class SaveRepositoryHandler : SaveHandler<VstsRepositoryViewModel, Repository, SaveRepository>
    {
        public SaveRepositoryHandler(IRepository repository, IMapper mapper)
            : base(repository, mapper)
        {
        }

        protected override Task<VstsRepositoryViewModel> FixViewModel(SaveRepository command) => Task.FromResult(command.Repository);

        protected override void ValidateCommand(SaveRepository command)
        {
            if (command.Repository == null)
            {
                throw new ArgumentNullException(nameof(command.Repository));
            }
        }
    }
}
