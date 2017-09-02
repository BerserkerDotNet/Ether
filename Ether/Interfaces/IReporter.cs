using Ether.Types.DTO.Reports;
using System.Threading.Tasks;

namespace Ether.Interfaces
{
    public interface IReporter
    {
        string Name { get; }
        Task<ReportResult> ReportAsync(ReportQuery query);
    }
}
