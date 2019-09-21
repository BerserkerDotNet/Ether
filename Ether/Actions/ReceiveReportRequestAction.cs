using BlazorState.Redux.Interfaces;
using Ether.ViewModels;

namespace Ether.Actions
{
    public class ReceiveReportRequestAction : IAction
    {
        public GenerateReportViewModel Request { get; set; }
    }
}
