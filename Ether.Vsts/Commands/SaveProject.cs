using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;

namespace Ether.Vsts.Commands
{
    public class SaveProject : ICommand
    {
        public VstsProjectViewModel Project { get; set; }
    }
}
