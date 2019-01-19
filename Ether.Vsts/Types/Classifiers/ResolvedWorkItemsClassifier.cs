using System;
using System.Linq;
using Ether.Contracts.Types;
using Ether.ViewModels;
using static Ether.Vsts.Constants;

namespace Ether.Vsts.Types.Classifiers
{
    public class ResolvedWorkItemsClassifier : BaseWorkItemsClassifier
    {
        protected override WorkItemResolution ClassifyInternal(WorkItemResolutionRequest request)
        {
            var resolutionUpdate = request.WorkItem.Updates.LastOrDefault(u => u[WorkItemStateField].NewValue == WorkItemStateResolved
                && u[WorkItemStateField].OldValue != WorkItemStateClosed
                && request.Team.Any(member => !string.IsNullOrEmpty(u[WorkItemResolvedByField].NewValue) && u[WorkItemResolvedByField].NewValue.Contains(member.Email)));
            if (resolutionUpdate == null)
            {
                return WorkItemResolution.None;
            }

            var assignedToMember = request.Team.SingleOrDefault(member => !resolutionUpdate[WorkItemAssignedToField].IsEmpty() &&
                !string.IsNullOrEmpty(resolutionUpdate[WorkItemAssignedToField].OldValue) &&
                resolutionUpdate[WorkItemAssignedToField].OldValue.Contains(member.Email));
            var resolvedByMemeber = request.Team.Single(member => resolutionUpdate[WorkItemResolvedByField].NewValue.Contains(member.Email));
            if (assignedToMember != null)
            {
                resolvedByMemeber = assignedToMember;
            }

            return new WorkItemResolution(
                request.WorkItem.WorkItemId,
                request.WorkItem[WorkItemTitleField],
                request.WorkItem[WorkItemTypeField],
                WorkItemStateResolved,
                resolutionUpdate[WorkItemReasonField].NewValue,
                DateTime.Parse(resolutionUpdate[WorkItemChangedDateField].NewValue),
                resolvedByMemeber.Email,
                resolvedByMemeber.DisplayName);
        }

        protected override bool IsSupported(WorkItemViewModel item)
        {
            var type = item[WorkItemTypeField];
            return string.Equals(type, WorkItemTypeBug, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(type, WorkItemTypeTask, StringComparison.OrdinalIgnoreCase);
        }
    }
}
