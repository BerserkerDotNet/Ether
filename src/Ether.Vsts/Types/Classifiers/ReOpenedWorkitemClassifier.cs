using System;
using System.Collections.Generic;
using System.Linq;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Types;
using Ether.ViewModels;

namespace Ether.Vsts.Types.Classifiers
{
    public class ReOpenedWorkitemClassifier : VstsBaseWorkItemsClassifier
    {
        protected override IEnumerable<IWorkItemEvent> ClassifyInternal(WorkItemResolutionRequest request)
        {
            var wrapper = GetWorkItemWrapper(request.WorkItem);
            var updates = wrapper.Updates;

            var reOpens = updates.Where(u => IsResolved(u.State.Old));
            foreach (var reOpen in reOpens)
            {
                var previousUpdates = updates.TakeWhile(u => u != reOpen);
                var resolvedUpdate = previousUpdates.Last(u => IsResolved(u.State.New));

                yield return new WorkItemReOpenedEvent(wrapper, reOpen.ChangedDate.New, resolvedUpdate.ResolvedBy.New);
            }
        }

        protected override bool IsSupported(WorkItemViewModel item)
        {
            var wrapper = GetWorkItemWrapper(item);
            return string.Equals(wrapper.Type, Constants.WorkItemTypeBug, System.StringComparison.OrdinalIgnoreCase);
        }

        private bool IsResolved(string state)
        {
            return IsStateEqual(state, Constants.WorkItemStateResolved);
        }

        private bool IsActive(string state)
        {
            return IsStateEqual(state, Constants.WorkItemStateResolved);
        }

        private bool IsNew(string state)
        {
            return IsStateEqual(state, Constants.WorkItemStateResolved);
        }

        private bool IsStateEqual(string state, string expectedState)
        {
            return string.Equals(state, expectedState, StringComparison.OrdinalIgnoreCase);
        }
    }
}
