using System.Collections.Generic;
using Ether.Contracts.Types;
using Ether.ViewModels;

namespace Ether.Contracts.Interfaces
{
    public interface IWorkItemClassificationContext
    {
        IEnumerable<WorkItemResolution> Classify(WorkItemViewModel item, ClassificationScope scope);
    }
}
