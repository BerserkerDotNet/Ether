using System.Collections.Generic;
using Ether.ViewModels;

namespace Ether.Types.State
{
    public class ReportsState
    {
        public ReportsState(IEnumerable<ReportViewModel> reports)
        {
            Reports = reports;
        }

        public IEnumerable<ReportViewModel> Reports { get; private set; }
    }
}
