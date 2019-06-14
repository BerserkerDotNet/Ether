using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ether.Contracts.Types;
using Ether.ViewModels;
using Ether.ViewModels.Types;

namespace Ether.Contracts.Interfaces
{
    public interface IDataSource
    {
        Task<ProfileViewModel> GetProfile(Guid id);

        Task<IEnumerable<PullRequestViewModel>> GetPullRequests(Expression<Func<PullRequestViewModel, bool>> predicate);

        Task<TeamMemberViewModel> GetTeamMember(Guid id);

        ETAValues GetETAValues(WorkItemViewModel workItem);

        Task<IEnumerable<WorkItemViewModel>> GetWorkItemsFor(Guid memberId);

        float GetActiveDuration(WorkItemViewModel workItem);

        Task<bool> IsInCodeReview(WorkItemViewModel workItem);

        bool IsActive(WorkItemViewModel workItem);

        bool IsAssignedToTeamMember(WorkItemViewModel workItem, IEnumerable<TeamMemberViewModel> team);

        bool IsResolved(IEnumerable<WorkItemResolution> resolutions);

        WorkItemDetail CreateWorkItemDetail(WorkItemViewModel item);
    }
}
