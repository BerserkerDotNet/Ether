using Ether.Core.Models;

namespace Ether.Core.Interfaces
{
    public interface IWorkItemsClassifier
    {
        WorkItemResolution Classify(WorkItemResolutionRequest request);
    }
}
