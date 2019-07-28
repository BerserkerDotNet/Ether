using System;
using System.Collections.Generic;
using Ether.Contracts.Interfaces.CQS;
using Ether.Vsts.Types;

namespace Ether.Vsts.Queries
{
    public class GetMembersAndRepositoriesOfAllProfiles : IQuery<IEnumerable<RepositoryInfo>>
    {
        public GetMembersAndRepositoriesOfAllProfiles(IEnumerable<Guid> includeMembers)
        {
            IncludeMembers = includeMembers;
        }

        public IEnumerable<Guid> IncludeMembers { get; set; }
    }
}
