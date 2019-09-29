using System.Collections.Generic;
using Ether.Contracts.Types;
using Ether.ViewModels;

namespace Ether.Contracts.Interfaces
{
    public interface IWorkItemClassificationContext
    {
        IEnumerable<IWorkItemEvent> Classify(WorkItemViewModel item, ClassificationScope scope);
    }
}
