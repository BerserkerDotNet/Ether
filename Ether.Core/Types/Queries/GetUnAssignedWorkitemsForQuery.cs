using System;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;

namespace Ether.Core.Types.Queries
{
    public class GetUnAssignedWorkitemsForQuery : IQuery<UnAssignedWorkitemsViewModel>
    {
        public string DataSourceType { get; set; }

        public Guid QueryId { get; set; }
    }
}
