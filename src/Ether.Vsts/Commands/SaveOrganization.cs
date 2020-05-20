using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;

namespace Ether.Vsts.Commands
{
    public class SaveOrganization : ICommand
    {
        public OrganizationViewModel Organization { get; set; }
    }
}
