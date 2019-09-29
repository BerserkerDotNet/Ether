using System;

namespace Ether.ViewModels
{
    public class GenerateReportViewModel
    {
        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public Guid Profile { get; set; }

        public string ReportType { get; set; }
    }
}
