using System;
using Ether.Contracts.Interfaces;
using Ether.ViewModels;

namespace Ether.Contracts.Types
{
    public class BaseWorkItemEvent : IWorkItemEvent
    {
        public BaseWorkItemEvent(IWorkItem workItem, DateTime date, UserReference user)
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
