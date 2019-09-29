using System;
using System.Collections.Generic;
using System.Linq;
using Ether.Contracts.Types;
using Ether.ViewModels;
using static Ether.Vsts.Constants;

namespace Ether.Vsts.Types.Classifiers
{
    public class ResolvedWorkItemsClassifier : BaseWorkItemsClassifier
    {
        protected override IEnumerable<WorkItemResolution> ClassifyInternal(WorkItemResolutionRequest request)
        {
            var resolutionUpdate = request.WorkItem.Updates.LastOrDefault(u => WasResolvedByTeamMember(u, request));
            if (resolutionUpdate == null)
            {
                return Enumerable.Empty<WorkItemResolution>();
            }

            var assignedToMember = request.Team.SingleOrDefault(member => !resolutionUpdate[WorkItemAssignedToField].IsEmpty() &&
                !string.IsNullOrEmpty(resolutionUpdate[WorkItemAssignedToField].OldValue) &&
                resolutionUpdate[WorkItemAssignedToField].OldValue.Contains(member.Email));
            if (assignedToMember == null)
            {
                assignedToMember = request.Team.SingleOrDefault(t => (!string.IsNullOrEmpty(request.WorkItem[WorkItemAssignedToField]) && request.WorkItem[WorkItemAssignedToField].Contains(t.Email)));
            }

            var resolvedByMemeber = request.Team.SingleOrDefault(member => resolutionUpdate.Fields.ContainsKey(WorkItemResolvedByField) && resolutionUpdate[WorkItemResolvedByField].NewValue.Contains(member.Email));
            if (assignedToMember != null)
            {
                resolvedByMemeber = assignedToMember;
            }

            return new[]
            {
                new WorkItemResolution(
                    request.WorkItem.WorkItemId,
                    request.WorkItem[WorkItemTitleField],
                    request.WorkItem[WorkItemTypeField],
                    WorkItemStateResolved,
                    resolutionUpdate[WorkItemReasonField].NewValue,
                    DateTime.Parse(resolutionUpdate[WorkItemChangedDateField].NewValue),
                    resolvedByMemeber.Email,
                    resolvedByMemeber.DisplayName)
            };
        }

        protected override bool IsSupported(WorkItemViewModel item)
        {
            var type = item[WorkItemTypeField];
            return string.Equals(type, WorkItemTypeBug, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(type, WorkItemTypeTask, StringComparison.OrdinalIgnoreCase);
        }

        private bool WasResolvedByTeamMember(WorkItemUpdateViewModel update, WorkItemResolutionRequest request)
        {
            var assignedTo = update[WorkItemAssignedToField].IsEmpty() ? request.WorkItem[WorkItemAssignedToField] : update[WorkItemAssignedToField].OldValue;
            var resolvedBy = update[WorkItemResolvedByField].NewValue;
            return update[WorkItemStateField].NewValue == WorkItemStateResolved
                && update[WorkItemStateField].OldValue != WorkItemStateClosed
                && request.Team.Any(t => (!string.IsNullOrEmpty(assignedTo) && assignedTo.Contains(t.Email)) || (!string.IsNullOrEmpty(resolvedBy) && resolvedBy.Contains(t.Email)));
        }
    }
}
