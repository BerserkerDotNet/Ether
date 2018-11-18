using System.Collections.Generic;
using Ether.Contracts.Interfaces.CQS;
using Ether.Vsts.Types;

namespace Ether.Vsts.Queries
{
    public class GetMembersAndRepositoriesOfAllProfiles : IQuery<IEnumerable<RepositoryInfo>>
    {
    }
}
