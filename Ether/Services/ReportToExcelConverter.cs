using Ether.Core.Models.DTO.Reports;
using System;

namespace Ether.Services
{
    public abstract class ReportToExcelConverter
    {
        public abstract byte[] Convert(ReportResult report);

        public static ReportToExcelConverter GetConverter(Type reportType)
        {
            if (reportType == typeof(PullRequestsReport))
                return new PullRequestsReportToExcelConverter();

            throw new NotSupportedException($"Report of type '{reportType}' cannot be converted to Excel.");
        }
    }
}
