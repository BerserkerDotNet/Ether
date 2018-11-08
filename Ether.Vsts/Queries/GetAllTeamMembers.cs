using System.Collections.Generic;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;

namespace Ether.Vsts.Queries
{
    public class GetAllTeamMembers : IQuery<IEnumerable<VstsTeamMemberViewModel>>
    {
    }
}
