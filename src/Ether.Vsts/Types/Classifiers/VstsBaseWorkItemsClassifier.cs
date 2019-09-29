using Ether.Contracts.Interfaces;
using Ether.Contracts.Types;
using Ether.ViewModels;

namespace Ether.Vsts.Types.Classifiers
{
    public abstract class VstsBaseWorkItemsClassifier : BaseWorkItemsClassifier
    {
        protected override IWorkItem GetWorkItemWrapper(WorkItemViewModel workItem)
        {
            return new VstsWorkItem(workItem);
        }
    }
}
