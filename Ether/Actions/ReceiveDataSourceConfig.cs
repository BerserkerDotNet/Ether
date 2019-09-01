using Ether.Redux.Interfaces;
using Ether.ViewModels;

namespace Ether.Actions
{
    public class ReceiveDataSourceConfig : IAction
    {
        public VstsDataSourceViewModel Config { get; set; }
    }
}
