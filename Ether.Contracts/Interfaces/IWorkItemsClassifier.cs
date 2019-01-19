using Ether.Contracts.Types;

namespace Ether.Contracts.Interfaces
{
    public interface IWorkItemsClassifier
    {
        WorkItemResolution Classify(WorkItemResolutionRequest request);
    }
}
