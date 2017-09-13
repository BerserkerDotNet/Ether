using Ether.Core.Models;
using Ether.Core.Models.DTO.Reports;
using System;
using System.Threading.Tasks;

namespace Ether.Core.Interfaces
{
    public interface IReporter
    {
        string Name { get; }
        Guid Id { get; }
        Type ReportType { get; }
        Task<ReportResult> ReportAsync(ReportQuery query);
    }
}