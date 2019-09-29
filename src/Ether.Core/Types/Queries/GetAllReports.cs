using System.Collections.Generic;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;

namespace Ether.Core.Types.Queries
{
    public class GetAllReports : IQuery<IEnumerable<ReportViewModel>>
    {
    }
}
