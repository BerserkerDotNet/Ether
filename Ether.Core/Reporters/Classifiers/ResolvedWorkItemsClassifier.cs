using Ether.Core.Interfaces;
using Ether.Core.Models;
using System;
using System.Linq;

namespace Ether.Core.Reporters.Classifiers
{
    public class ResolvedWorkItemsClassifier : IWorkItemsClassifier
    {
        private static readonly string[] SupportedTypes = new[] { "Bug", "Task" };

        public WorkItemResolution Classify(WorkItemResolutionRequest request)
        {
            const string ResolvedState = "Resolved";
            const string ClosedState = "Closed";

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var workItem = request.WorkItem;
            if (workItem == null)
                return WorkItemResolution.None;

            if (!SupportedTypes.Contains(workItem.WorkItemType))
                return WorkItemResolution.None;

            var resolutionUpdate = workItem.Updates.LastOrDefault(u => u.State.NewValue == ResolvedState 
                    && u.State.OldValue != ClosedState
                    && request.Team.Any(t => !string.IsNullOrEmpty(u.ResolvedBy.NewValue) && u.ResolvedBy.NewValue.Contains(t.Email)));
            if (resolutionUpdate == null)
                return WorkItemResolution.None;

            var resolvedByMemeber = request.Team.Single(m => resolutionUpdate.ResolvedBy.NewValue.Contains(m.Email));
            return new WorkItemResolution(workItem, ResolvedState, resolutionUpdate.Reason.NewValue,
                resolutionUpdate.RevisedDate, resolvedByMemeber.Email, resolvedByMemeber.DisplayName);
        }
    }
}
