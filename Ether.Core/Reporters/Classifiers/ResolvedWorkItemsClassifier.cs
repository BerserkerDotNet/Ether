using Ether.Core.Constants;
using Ether.Core.Models;
using System.Linq;

namespace Ether.Core.Reporters.Classifiers
{
    public class ResolvedWorkItemsClassifier : BaseWorkItemsClassifier
    {
        public ResolvedWorkItemsClassifier()
            :base(WorkItemTypes.Bug, WorkItemTypes.Task)
        {

        }

        protected override WorkItemResolution ClassifyInternal(WorkItemResolutionRequest request)
        {
            var resolutionUpdate = request.WorkItem.Updates.LastOrDefault(u => u.State.NewValue == ResolvedState 
                    && u.State.OldValue != ClosedState
                    && request.Team.Any(t => !string.IsNullOrEmpty(u.ResolvedBy.NewValue) && u.ResolvedBy.NewValue.Contains(t.Email)));
            if (resolutionUpdate == null)
                return WorkItemResolution.None;

            var resolvedByMemeber = request.Team.Single(m => resolutionUpdate.ResolvedBy.NewValue.Contains(m.Email));
            return new WorkItemResolution(request.WorkItem, ResolvedState, resolutionUpdate.Reason.NewValue,
                resolutionUpdate.ChangedDate, resolvedByMemeber.Email, resolvedByMemeber.DisplayName);
        }
    }
}
