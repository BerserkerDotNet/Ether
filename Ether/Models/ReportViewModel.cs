using System;

namespace Ether.Models
{
    public class ReportViewModel
    {
        public Guid Profile { get; set; }
        public Guid Report { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
