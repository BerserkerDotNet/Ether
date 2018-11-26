using System;
using Ether.Contracts.Interfaces.CQS;

namespace Ether.Core.Types.Commands
{
    public class GeneratePullRequestsReport : ICommand<Guid>
    {
        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public Guid Profile { get; set; }

        public string DataSourceType { get; set; }
    }
}
