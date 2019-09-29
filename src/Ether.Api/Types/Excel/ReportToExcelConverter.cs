using Ether.ViewModels;

namespace Ether.Types.Excel
{
    public abstract class ReportToExcelConverter
    {
        public abstract byte[] Convert(ReportViewModel report);
    }
}
