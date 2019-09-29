using System;
using System.Collections.Generic;
using System.Linq;
using Ether.Contracts.Interfaces;
using Ether.ViewModels;

namespace Ether.Contracts.Types
{
    public abstract class BaseWorkItemsClassifier : IWorkItemsClassifier
    {
        public IEnumerable<IWorkItemEvent> Classify(WorkItemResolutionRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var workItem = request.WorkItem;
            if (workItem == null || workItem.Updates == null || !workItem.Updates.Any())
            {
                return Enumerable.Empty<IWorkItemEvent>();
            }

            if (!IsSupported(workItem))
            {
                return Enumerable.Empty<IWorkItemEvent>();
            }

            try
            {
                return ClassifyInternal(request);
            }
            catch (Exception ex)
            {
                return new[] { new ErrorClassifyingWorkItemEvent(GetWorkItemWrapper(workItem), this, ex) };
            }
        }

        protected abstract bool IsSupported(WorkItemViewModel item);

        // HACK: Replace WorkITemViewModel completely witrh IWorkItem
        protected abstract IWorkItem GetWorkItemWrapper(WorkItemViewModel workItem);

        protected abstract IEnumerable<IWorkItemEvent> ClassifyInternal(WorkItemResolutionRequest request);
    }
}
