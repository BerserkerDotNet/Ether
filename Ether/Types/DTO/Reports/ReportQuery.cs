using System;

namespace Ether.Types.DTO.Reports
{
    public class ReportQuery
    {
        public Guid ProfileId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
