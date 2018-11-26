using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ether.ViewModels;

namespace Ether.Contracts.Interfaces
{
    public interface IDataSource
    {
        Task<ProfileViewModel> GetProfile(Guid id);

        Task<IEnumerable<PullRequestViewModel>> GetPullRequests(Expression<Func<PullRequestViewModel, bool>> predicate);

        Task<TeamMemberViewModel> GetTeamMember(Guid id);
    }
}
