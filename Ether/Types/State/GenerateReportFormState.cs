using System;
using System.Collections.Generic;
using Ether.ViewModels;
using Newtonsoft.Json;

namespace Ether.Types.State
{
    public class GenerateReportFormState
    {
        [JsonConstructor]
        public GenerateReportFormState(GenerateReportViewModel form, IEnumerable<ReporterDescriptorViewModel> reportTypes)
        {
            Form = form;
            ReportTypes = reportTypes;
        }

        public GenerateReportFormState(IEnumerable<ReporterDescriptorViewModel> reportTypes)
            : this(null, reportTypes)
        {
        }

        public GenerateReportViewModel Form { get; set; }

        public IEnumerable<ReporterDescriptorViewModel> ReportTypes { get; set; }
    }

}
