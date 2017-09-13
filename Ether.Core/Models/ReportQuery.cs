using System;

namespace Ether.Core.Models
{
    public class ReportQuery
    {
        public Guid ProfileId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
