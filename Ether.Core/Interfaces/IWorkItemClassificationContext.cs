using System.Collections.Generic;
using Ether.Core.Models;
using Ether.Core.Models.VSTS;
using Ether.Core.Types;

namespace Ether.Core.Interfaces
{
    public interface IWorkItemClassificationContext
    {
        IEnumerable<WorkItemResolution> Classify(VSTSWorkItem item, ClassificationScope scope);
    }
}