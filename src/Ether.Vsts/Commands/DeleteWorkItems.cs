using System.Collections.Generic;
using Ether.Contracts.Interfaces.CQS;

namespace Ether.Vsts.Commands
{
    public class DeleteWorkItems : ICommand
    {
        public DeleteWorkItems(IEnumerable<int> ids)
        {
            Ids = ids;
        }

        public IEnumerable<int> Ids { get; }
    }
}
