using System;
using Ether.ViewModels;

namespace Ether.Contracts.Interfaces
{
    public interface IWorkItemEvent
    {
        IWorkItem WorkItem { get; }

        DateTime Date { get; }

        UserReference AssociatedUser { get; }
    }
}
