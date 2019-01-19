using System;
using System.Collections.Generic;
using System.Linq;
using Ether.Contracts.Types;
using Ether.ViewModels;
using static Ether.Vsts.Constants;

namespace Ether.Vsts.Types.Classifiers
{
    public class ClosedTasksWorkItemsClassifier : BaseWorkItemsClassifier
    {
        protected override WorkItemResolution ClassifyInternal(WorkItemResolutionRequest request)
        {
            var resolutionUpdate = request.WorkItem.Updates.LastOrDefault(u => WasClosedByTeamMember(u, request.Team));
            var wasEverResolved = request.WorkItem.Updates.Any(u => u[WorkItemStateField].NewValue == WorkItemStateResolved);
            if (resolutionUpdate == null || wasEverResolved)
            {
                return WorkItemResolution.None;
            }

            var reason = resolutionUpdate[WorkItemReasonField].NewValue;
            var assignedToMember = request.Team.SingleOrDefault(m => !resolutionUpdate[WorkItemAssignedToField].IsEmpty() &&
                !string.IsNullOrEmpty(resolutionUpdate[WorkItemAssignedToField].OldValue) &&
                resolutionUpdate[WorkItemAssignedToField].OldValue.Contains(m.Email));
            var closedByMemeber = request.Team.Single(m => resolutionUpdate[WorkItemClosedByField].NewValue.Contains(m.Email));
            if (assignedToMember != null)
            {
                closedByMemeber = assignedToMember;
            }

            return new WorkItemResolution(
                request.WorkItem.WorkItemId,
                request.WorkItem[WorkItemTitleField],
                request.WorkItem[WorkItemTypeField],
                WorkItemStateClosed,
                reason,
                DateTime.Parse(resolutionUpdate[WorkItemChangedDateField].NewValue),
                closedByMemeber.Email,
                closedByMemeber.DisplayName);
        }

        protected override bool IsSupported(WorkItemViewModel item)
        {
            var type = item[WorkItemTypeField];
            return string.Equals(type, WorkItemTypeTask, StringComparison.OrdinalIgnoreCase);
        }

        private bool WasClosedByTeamMember(WorkItemUpdateViewModel update, IEnumerable<TeamMemberViewModel> team)
        {
            var closedBy = update[WorkItemClosedByField].NewValue;
            return update[WorkItemStateField].NewValue == WorkItemStateClosed
                && update[WorkItemStateField].OldValue != WorkItemStateResolved
                && team.Any(t => !string.IsNullOrEmpty(closedBy) && closedBy.Contains(t.Email));
        }
    }
}
