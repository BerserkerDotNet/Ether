using System;
using System.Linq;
using Ether.Contracts.Interfaces;
using Ether.ViewModels;

namespace Ether.Contracts.Types
{
    public abstract class BaseWorkItemsClassifier : IWorkItemsClassifier
    {
        public WorkItemResolution Classify(WorkItemResolutionRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var workItem = request.WorkItem;
            if (workItem == null || workItem.Updates == null || !workItem.Updates.Any())
            {
                return WorkItemResolution.None;
            }

            if (!IsSupported(workItem))
            {
                return WorkItemResolution.None;
            }

            try
            {
                return ClassifyInternal(request);
            }
            catch (Exception ex)
            {
                return WorkItemResolution.GetError(ex);
            }
        }

        protected abstract bool IsSupported(WorkItemViewModel item);

        protected abstract WorkItemResolution ClassifyInternal(WorkItemResolutionRequest request);
    }
}
