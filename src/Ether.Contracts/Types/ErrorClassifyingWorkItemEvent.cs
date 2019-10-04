using System;
using Ether.Contracts.Interfaces;
using Ether.ViewModels;

namespace Ether.Contracts.Types
{
    public class ErrorClassifyingWorkItemEvent : IWorkItemEvent
    {
        public ErrorClassifyingWorkItemEvent(IWorkItem workItem, IWorkItemsClassifier classifier, Exception error)
        {
            WorkItem = workItem;
            Classifier = classifier;
            Error = error;
            Date = DateTime.UtcNow;
        }

        public IWorkItem WorkItem { get; }

        public IWorkItemsClassifier Classifier { get; }

        public Exception Error { get; }

        public DateTime Date { get; }

        public UserReference AssociatedUser => null;
    }
}
