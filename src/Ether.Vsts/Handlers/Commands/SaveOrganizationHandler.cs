using System;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Interfaces;
using Ether.Core.Types.Commands;
using Ether.ViewModels;
using Ether.Vsts.Commands;
using Ether.Vsts.Dto;

namespace Ether.Vsts.Handlers.Commands
{
    public class SaveOrganizationHandler : SaveHandler<OrganizationViewModel, VstsOrganization, SaveOrganization>
    {
        public SaveOrganizationHandler(IRepository repository, IMapper mapper)
            : base(repository, mapper)
        {
        }

        protected override void ValidateCommand(SaveOrganization command)
        {
            if (command.Organization == null)
            {
                throw new ArgumentNullException(nameof(command.Organization));
            }
        }

        protected override Task<OrganizationViewModel> FixViewModel(SaveOrganization command) => Task.FromResult(command.Organization);
    }
}
