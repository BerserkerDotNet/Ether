using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;

namespace Ether.Vsts.Commands
{
    public class SaveRepository : ICommand
    {
        public VstsRepositoryViewModel Repository { get; set; }
    }
}
