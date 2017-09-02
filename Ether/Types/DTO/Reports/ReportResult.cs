using System;

namespace Ether.Types.DTO.Reports
{
    public class ReportResult : BaseDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ProfileName { get; set; }
        public DateTime DateTaken { get; set; }
        public string ReportType { get; set; }
        public string ReportName { get; set; }
    }
}
