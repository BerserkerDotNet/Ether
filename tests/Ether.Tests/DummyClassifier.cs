using System;
using System.Collections.Generic;
using Ether.Contracts.Types;
using Ether.ViewModels;
using Ether.Vsts;

namespace Ether.Tests.Classifiers
{
    public class DummyClassifier : BaseWorkItemsClassifier
    {
        public const string SupportedType = "Bar";
        public const string ExpectedResolution = "Dummy";
        private readonly string _resolution;
        private readonly string _type;
        private readonly Action<WorkItemResolutionRequest> _requestProcessor;

        public DummyClassifier(string resolution = ExpectedResolution, string type = SupportedType, Action<WorkItemResolutionRequest> requestProcessor = null)
        {
            _resolution = resolution;
            _type = type;
            _requestProcessor = requestProcessor;
        }

        protected override IEnumerable<WorkItemResolution> ClassifyInternal(WorkItemResolutionRequest request)
        {
            _requestProcessor?.Invoke(request);

            return new[] { new WorkItemResolution(request.WorkItem.WorkItemId, request.WorkItem["Title"], request.WorkItem[Constants.WorkItemTypeField], _resolution, "Because", DateTime.UtcNow.AddDays(-1), string.Empty, string.Empty) };
        }

        protected override bool IsSupported(WorkItemViewModel item)
        {
            return item[Constants.WorkItemTypeField] == _type;
        }
    }
}
