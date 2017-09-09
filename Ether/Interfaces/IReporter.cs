using Ether.Types.DTO.Reports;
using System;
using System.Threading.Tasks;

namespace Ether.Interfaces
{
    public interface IReporter
    {
        string Name { get; }
        Guid Id { get; }
        Type ReportType { get; }
        Task<ReportResult> ReportAsync(ReportQuery query);
    }
}