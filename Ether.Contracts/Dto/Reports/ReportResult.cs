using System;

namespace Ether.Contracts.Dto.Reports
{
    public class ReportResult : BaseDto
    {
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public Guid ProfileId { get; set; }

        public string ProfileName { get; set; }

        public DateTime DateTaken { get; set; }

        public string ReportType { get; set; }

        public string ReportName { get; set; }

        public TimeSpan? GeneratedIn { get; set; }
    }
}
