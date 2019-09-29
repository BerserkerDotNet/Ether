using System.Collections.Generic;
using Ether.ViewModels;
using Newtonsoft.Json;

namespace Ether.Types.State
{
    public class ReportsState
    {
        [JsonConstructor]
        public ReportsState(IEnumerable<ReportViewModel> reports)
        {
            Reports = reports;
        }

        public IEnumerable<ReportViewModel> Reports { get; private set; }
    }
}
