using Ether.Core.Interfaces;
using Ether.Core.Models;
using System;
using System.Linq;

namespace Ether.Core.Reporters.Classifiers
{
    public abstract class BaseWorkItemsClassifier : IWorkItemsClassifier
    {
        private readonly string[] _supportedTypes;

        protected const string ResolvedState = "Resolved";
        protected const string ClosedState = "Closed";

        public BaseWorkItemsClassifier(params string[] supportedTypes)
        {
            _supportedTypes = supportedTypes;
        }

        public WorkItemResolution Classify(WorkItemResolutionRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var workItem = request.WorkItem;
            if (workItem == null || workItem.Updates == null || !workItem.Updates.Any())
                return WorkItemResolution.None;

            if (!_supportedTypes.Contains(workItem.WorkItemType))
                return WorkItemResolution.None;

            return ClassifyInternal(request);
        }

        protected abstract WorkItemResolution ClassifyInternal(WorkItemResolutionRequest request);
    }
}
