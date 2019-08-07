using System.Collections.Generic;
using Ether.ViewModels.Types;

namespace Ether.ViewModels
{
    public class WorkItemUpdateRelations
    {
        public IEnumerable<WorkItemRelation> Added { get; set; }

        public IEnumerable<WorkItemRelation> Removed { get; set; }

        public IEnumerable<WorkItemRelation> Updated { get; set; }
    }
}
