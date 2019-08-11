using System;
using System.Linq;

namespace Ether.ViewModels
{
    public class ReportViewModel : ViewModelWithId
    {
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string ProfileName { get; set; }

        public Guid ProfileId { get; set; }

        public DateTime DateTaken { get; set; }

        public string ReportType { get; set; }

        public string ReportName { get; set; }

        public TimeSpan? GeneratedIn { get; set; }
    }
}
