using System.Collections.Generic;
using Ether.Contracts.Dto;
using Ether.ViewModels;

namespace Ether.Vsts.Dto
{
    public class WorkItem : BaseDto
    {
        public int WorkItemId { get; set; }

        public Dictionary<string, string> Fields { get; set; }

        public IEnumerable<WorkItemUpdateViewModel> Updates { get; set; }
    }
}
