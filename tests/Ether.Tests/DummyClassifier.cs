using System;
using System.Collections.Generic;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Types;
using Ether.ViewModels;
using Ether.Vsts;
using Ether.Vsts.Types;

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

        protected override IEnumerable<IWorkItemEvent> ClassifyInternal(WorkItemResolutionRequest request)
        {
            _requestProcessor?.Invoke(request);

            return new[] { new DummyWorkItemEvent(GetWorkItemWrapper(request.WorkItem), DateTime.UtcNow.AddDays(-1), new UserReference { Email = string.Empty, Title = string.Empty }) };
        }

        protected override IWorkItem GetWorkItemWrapper(WorkItemViewModel workItem)
        {
            return new VstsWorkItem(workItem);
        }

        protected override bool IsSupported(WorkItemViewModel item)
        {
            return item[Constants.WorkItemTypeField] == _type;
        }
    }

    public class DummyWorkItemEvent : IWorkItemEvent
    {
        public DummyWorkItemEvent(IWorkItem workItem, DateTime date, UserReference user)
        {
            WorkItem = workItem;
            Date = date;
            AssociatedUser = user;
        }

        public IWorkItem WorkItem { get; }

        public DateTime Date { get; }

        public UserReference AssociatedUser { get; }
    }
}
