using System.Collections.Generic;
using Ether.Redux.Interfaces;
using Ether.ViewModels;

namespace Ether.Actions
{
    public class ReceiveReportsAction : IAction
    {
        public IEnumerable<ReportViewModel> Reports { get; set; }
    }
}
