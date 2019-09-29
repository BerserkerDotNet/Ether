using System;
using Ether.Contracts.Interfaces;
using Ether.ViewModels;

namespace Ether.Contracts.Types
{
    public class WorkItemReOpenedEvent : BaseWorkItemEvent
    {
        public WorkItemReOpenedEvent(IWorkItem workItem, DateTime date, UserReference user)
            : base(workItem, date, user)
        {
        }
    }
}
