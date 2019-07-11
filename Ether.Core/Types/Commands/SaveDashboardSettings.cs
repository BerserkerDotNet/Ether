using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;

namespace Ether.Core.Types.Commands
{
    public class SaveDashboardSettings : ICommand
    {
        public DashboardSettingsViewModel Settings { get; set; }
    }
}
