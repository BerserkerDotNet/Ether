using System;
using System.Collections.Generic;
using Ether.Contracts.Dto;

namespace Ether.Vsts.Dto
{
    [Serializable]
    public class WorkItem : BaseDto
    {
        public int WorkItemId { get; set; }

        public Dictionary<string, string> Fields { get; set; }

        public IEnumerable<WorkItemUpdate> Updates { get; set; }
    }
}
