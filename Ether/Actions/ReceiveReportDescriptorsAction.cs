using System.Collections.Generic;
using BlazorState.Redux.Interfaces;
using Ether.ViewModels;

namespace Ether.Actions
{
    public class ReceiveReportDescriptorsAction : IAction
    {
        public IEnumerable<ReporterDescriptorViewModel> ReportDescriptors { get; set; }
    }
}
