using System.Collections.Generic;
using Ether.Redux.Interfaces;
using Ether.ViewModels;

namespace Ether.Actions
{
    public class ReceiveReportDescriptorsAction : IAction
    {
        public IEnumerable<ReporterDescriptorViewModel> ReportDescriptors { get; set; }
    }
}
