using System.Collections.Generic;
using Ether.Contracts.Types;

namespace Ether.Contracts.Interfaces
{
    public interface IWorkItemsClassifier
    {
        IEnumerable<IWorkItemEvent> Classify(WorkItemResolutionRequest request);
    }
}
