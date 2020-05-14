using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;

namespace Ether.Core.Types.Commands
{
    public class SaveOrganization : ICommand
    {
        public OrganizationViewModel Organization { get; set; }
    }
}
