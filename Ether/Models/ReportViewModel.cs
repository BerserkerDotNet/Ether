using System;
using System.Collections.Generic;

namespace Ether.Models
{
    public class ReportViewModel
    {
        public IEnumerable<Guid> Profiles { get; set; }

        public Guid Report { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
