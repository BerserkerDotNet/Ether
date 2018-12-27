using System.Collections.Generic;
using VSTS.Net.Models.WorkItems;

namespace Ether.ViewModels
{
    public class WorkItemViewModel : ViewModelWithId
    {
        public int WorkitemId => WorkItem.Id;

        public WorkItem WorkItem { get; set; }

        public IEnumerable<WorkItemUpdate> Updates { get; set; }
    }
}
