using System;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Interfaces;
using Ether.ViewModels;
using Ether.Vsts.Commands;
using Ether.Vsts.Dto;

namespace Ether.Vsts.Handlers.Commands
{
    public class SaveProfileHandler : SaveHandler<ProfileViewModel, VstsProfile, SaveProfile>
    {
        public SaveProfileHandler(IRepository repository, IMapper mapper)
            : base(repository, mapper)
        {
        }

        protected override void ValidateCommand(SaveProfile command)
        {
            if (command.Profile == null)
            {
                throw new ArgumentNullException(nameof(command.Profile));
            }
        }

        protected override Task<ProfileViewModel> GetData(SaveProfile command) => Task.FromResult(command.Profile);
    }
}
