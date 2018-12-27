using System.Collections.Generic;
using Ether.Contracts.Dto;

namespace Ether.Vsts.Dto
{
    public class WorkItem : BaseDto
    {
        public int WorkItemId { get; set; }

        public VSTS.Net.Models.WorkItems.WorkItem CurrentState { get; set; }

        public IEnumerable<VSTS.Net.Models.WorkItems.WorkItemUpdate> Updates { get; set; }
    }
}
