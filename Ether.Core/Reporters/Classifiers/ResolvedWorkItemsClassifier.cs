using Ether.Core.Interfaces;
using Ether.Core.Models;
using System;
using System.Linq;

namespace Ether.Core.Reporters.Classifiers
{
    public class ResolvedWorkItemsClassifier : IWorkItemsClassifier
    {
        public WorkItemResolution Classify(WorkItemResolutionRequest request)
        {
            const string ResolvedState = "Resolved";

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var supportedTypes = new[] { "Bug", "Task" };
            if (!supportedTypes.Contains(request.WorkItemType))
                return WorkItemResolution.None;

            var resolutionUpdate = request.WorkItemUpdates.LastOrDefault(u => u.State.NewValue == ResolvedState);
            if (resolutionUpdate == null)
                return WorkItemResolution.None;

            var resolution = new WorkItemResolution(ResolvedState, resolutionUpdate.Reason.NewValue, resolutionUpdate.RevisedDate, resolutionUpdate.ResolvedBy.NewValue);
            return resolution;
        }
    }
}
