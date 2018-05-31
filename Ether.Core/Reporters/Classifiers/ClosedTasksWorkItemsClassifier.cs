using Ether.Core.Constants;
using Ether.Core.Models;
using System.Linq;
using Ether.Core.Models.VSTS;
using System.Collections.Generic;
using Ether.Core.Models.DTO;

namespace Ether.Core.Reporters.Classifiers
{
    public class ClosedTasksWorkItemsClassifier : BaseWorkItemsClassifier
    {
        public ClosedTasksWorkItemsClassifier()
            : base(WorkItemTypes.Task)
        {

        }

        protected override WorkItemResolution ClassifyInternal(WorkItemResolutionRequest request)
        {
            var resolutionUpdate = request.WorkItem.Updates.LastOrDefault(u => WasClosedByTeamMember(u, request.Team));
            var wasEverResolved = request.WorkItem.Updates.Any(u => u.State.NewValue == WorkItemStates.Resolved);
            if (resolutionUpdate == null || wasEverResolved)
                return WorkItemResolution.None;

            var reason = resolutionUpdate.Reason.NewValue;
            var closedByMemeber = request.Team.Single(m => resolutionUpdate.ClosedBy.NewValue.Contains(m.Email));
            return new WorkItemResolution(request.WorkItem, WorkItemStates.Closed, reason, resolutionUpdate.ChangedDate, closedByMemeber.Email, closedByMemeber.DisplayName);
        }

        private bool WasClosedByTeamMember(WorkItemUpdate update, IEnumerable<TeamMember> team)
        {
            var closedBy = update.ClosedBy.NewValue;
            return update.State.NewValue == WorkItemStates.Closed
                && update.State.OldValue != WorkItemStates.Resolved
                && team.Any(t => !string.IsNullOrEmpty(closedBy) && closedBy.Contains(t.Email));
        }
    }
}
