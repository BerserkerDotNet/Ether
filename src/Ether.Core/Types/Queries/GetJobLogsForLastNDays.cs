using System.Collections.Generic;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;

namespace Ether.Core.Types.Queries
{
    public class GetJobLogsForLastNDays : IQuery<IEnumerable<JobLogViewModel>>
    {
        public GetJobLogsForLastNDays(int days)
        {
            Days = days;
        }

        public int Days { get; }
    }
}
