using System;
using System.Collections.Generic;
using System.Linq;
using Ether.Contracts.Interfaces;
using Ether.ViewModels;

namespace Ether.Contracts.Types
{
    public abstract class BaseWorkItemsClassifier : IWorkItemsClassifier
    {
        public IEnumerable<WorkItemResolution> Classify(WorkItemResolutionRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var workItem = request.WorkItem;
            if (workItem == null || workItem.Updates == null || !workItem.Updates.Any())
            {
                return Enumerable.Empty<WorkItemResolution>();
            }

            if (!IsSupported(workItem))
            {
                return Enumerable.Empty<WorkItemResolution>();
            }

            try
            {
                return ClassifyInternal(request);
            }
            catch (Exception ex)
            {
                return new[] { WorkItemResolution.GetError(ex) };
            }
        }

        protected abstract bool IsSupported(WorkItemViewModel item);

        protected abstract IEnumerable<WorkItemResolution> ClassifyInternal(WorkItemResolutionRequest request);
    }
}
