using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;

namespace Ether.Vsts.Commands
{
    public class SaveVstsDataSourceConfiguration : ICommand
    {
        public VstsDataSourceViewModel Configuration { get; set; }
    }
}
