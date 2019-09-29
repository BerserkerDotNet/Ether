using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;

namespace Ether.Core.Types.Commands
{
    public class SaveIdentity : ICommand
    {
        public IdentityViewModel Identity { get; set; }
    }
}
