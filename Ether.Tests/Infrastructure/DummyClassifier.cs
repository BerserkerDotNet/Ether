using Ether.Core.Models;
using Ether.Core.Reporters.Classifiers;
using System;

namespace Ether.Tests.Infrastructure
{
    public class DummyClassifier : BaseWorkItemsClassifier
    {
        public const string SupportedType = "Bar";
        public const string ExpectedResolution = "Dummy";
        private readonly string _resolution;
        private readonly Action<WorkItemResolutionRequest> _requestProcessor;

        public DummyClassifier(string resolution = ExpectedResolution, string type = SupportedType, Action<WorkItemResolutionRequest> requestProcessor = null)
            : base(type)
        {
            _resolution = resolution;
            _requestProcessor = requestProcessor;
        }

        protected override WorkItemResolution ClassifyInternal(WorkItemResolutionRequest request)
        {
            if (_requestProcessor != null)
                _requestProcessor(request);

            return new WorkItemResolution(request.WorkItem, _resolution, "Because", DateTime.UtcNow.AddDays(-1), string.Empty, string.Empty);
        }
    }
}
