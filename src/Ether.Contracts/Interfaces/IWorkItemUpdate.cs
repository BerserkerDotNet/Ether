using System;
using Ether.ViewModels;

namespace Ether.Contracts.Interfaces
{
    public interface IWorkItemUpdate
    {
        int Id { get; }

        int WorkItemId { get; }

        (string New, string Old) State { get; }

        (DateTime New, DateTime? Old) ChangedDate { get; }

        (UserReference New, UserReference Old) ResolvedBy { get; }

        (UserReference New, UserReference Old) ClosedBy { get; }

        (UserReference New, UserReference Old) AssignedTo { get; }
    }
}
